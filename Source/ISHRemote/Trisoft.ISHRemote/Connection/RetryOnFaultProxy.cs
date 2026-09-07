/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.ServiceModel;

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Transparent single-retry wrapper around a WCF SOAP channel interface (e.g. <c>Application25ServiceReference.Application</c>).
    /// Used by <see cref="InfoShareWcfSoapWithOpenIdConnectConnection"/> so that a channel that faults with
    /// "An unsecured or incorrectly secured fault was received from the other party" self-heals within the same cmdlet
    /// invocation, instead of requiring the caller to retry the cmdlet itself. See https://github.com/RWS/ISHRemote/issues/273.
    /// 
    /// Only wraps the plain service-contract interface, not <c>ICommunicationObject</c>/<c>IDisposable</c> - the channel
    /// state checks, Faulted event wiring and Dispose() bookkeeping in <c>InfoShareWcfSoapWithOpenIdConnectConnection</c>
    /// keep operating on the underlying raw channel field directly, this proxy is only applied at the point where the
    /// channel is handed out to callers.
    /// 
    /// Each public <c>Get*25Channel()</c> method is split into itself (which wraps exactly once) and a private
    /// <c>Ensure*25Channel()</c> helper (which does the state-check/rebuild and returns the raw, unwrapped channel). The
    /// rebuild delegate passed to <see cref="Wrap"/> must be that private helper - see the warning on <see cref="Wrap"/>.
    /// </summary>
    /// <typeparam name="T">The generated WCF service-contract interface, for example <c>Baseline25ServiceReference.Baseline</c>.</typeparam>
    /// <remarks>
    /// Must be <c>public</c>, not <c>internal</c>: on .NET Framework, <see cref="DispatchProxy.Create{T, TProxy}"/>
    /// generates the proxy type in a new dynamic assembly under strict Reflection.Emit visibility checks, so a
    /// non-public <c>TProxy</c> fails to load with "Access is denied" (observed on Windows PowerShell 5.1/net48).
    /// .NET 6.0+ relaxes this check, so the same code works there regardless of visibility - keep it public so both
    /// runtimes behave the same.
    /// </remarks>
    public class RetryOnFaultProxy<T> : DispatchProxy where T : class
    {
        private T _target;
        private Func<T> _rebuild;

        /// <summary>
        /// Wraps <paramref name="target"/> so that any call throwing <see cref="CommunicationException"/> or
        /// <see cref="FaultException"/> is retried exactly once against a freshly rebuilt channel obtained from
        /// <paramref name="rebuild"/>.
        /// </summary>
        /// <param name="target">Current channel instance, as currently held/rebuilt by the owning connection.</param>
        /// <param name="rebuild">Delegate that returns a (possibly rebuilt) channel of type <typeparamref name="T"/>. This
        /// MUST be the raw, unwrapped channel accessor (e.g. a private <c>Ensure*25Channel()</c> helper) - never the public
        /// <c>Get*25Channel()</c> method that itself calls <see cref="Wrap"/>. Passing a rebuild delegate that returns
        /// another wrapped proxy causes each retry to recurse into a brand-new <see cref="RetryOnFaultProxy{T}"/> with its
        /// own full retry budget; on a persistent (non-transient) fault this chains unbounded and causes a stack overflow
        /// instead of a single bounded retry.</param>
        /// <returns>A proxy implementing <typeparamref name="T"/> that transparently retries once on fault.</returns>
        public static T Wrap(T target, Func<T> rebuild)
        {
            object proxy = Create<T, RetryOnFaultProxy<T>>();
            var retryOnFaultProxy = (RetryOnFaultProxy<T>)proxy;
            retryOnFaultProxy._target = target;
            retryOnFaultProxy._rebuild = rebuild;
            return (T)proxy;
        }

        /// <inheritdoc/>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                return targetMethod.Invoke(_target, args);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is CommunicationException || tie.InnerException is FaultException)
            {
                // The current channel just faulted; Get*25Channel() rebuild logic will detect the Faulted state and
                // hand back a fresh channel. Retry exactly once, then let any further failure propagate as-is.
                _target = _rebuild();
                try
                {
                    return targetMethod.Invoke(_target, args);
                }
                catch (TargetInvocationException retryTie)
                {
                    // Unwrap so callers keep seeing the original exception type (e.g. FaultException) instead of
                    // reflection's TargetInvocationException wrapper, preserving the original stack trace.
                    ExceptionDispatchInfo.Capture(retryTie.InnerException ?? retryTie).Throw();
                    throw; // unreachable, ExceptionDispatchInfo.Throw() always throws
                }
            }
            catch (TargetInvocationException tie)
            {
                // Non-transient failure (or unwrap of any other reflected exception) - unwrap for the same reason as above.
                ExceptionDispatchInfo.Capture(tie.InnerException ?? tie).Throw();
                throw; // unreachable, ExceptionDispatchInfo.Throw() always throws
            }
        }
    }
}
