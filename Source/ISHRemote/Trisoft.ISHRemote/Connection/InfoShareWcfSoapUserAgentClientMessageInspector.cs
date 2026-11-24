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

#if !NET48
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
#endif

namespace Trisoft.ISHRemote.Connection
{
    /// <summary>
    /// Mutable holder for a lazily-decided User-Agent value. Reference type on purpose: <see cref="Objects.Public.IshSession"/>
    /// creates exactly one instance and hands it to every <see cref="InfoShareWcfSoapWithOpenIdConnectConnection"/>
    /// endpoint behavior at construction time. If a cloud WAF (AWS WAF Bot Control, Azure Front Door/App Gateway
    /// WAF bot manager, GCP Cloud Armor) later blocks a bare/no User-Agent request, <see cref="Objects.Public.IshSession"/> flips
    /// <see cref="Value"/> once - every already-constructed InfoShareWcfSoapUserAgentClientMessageInspector
    /// observes the change immediately on its next outgoing message, without rebuilding any channel. A plain
    /// <c>string</c> field could not do this: strings are immutable, so passing one at construction time only ever
    /// shares a snapshot, never the later mutation. See #275.
    /// </summary>
    /// <remarks>
    /// Declared unconditionally (not under <c>#if !NET48</c>) because <see cref="Objects.Public.IshSession"/> - which builds for
    /// net48/net6.0/net10.0 alike - owns and mutates the single instance from its HttpClient-based
    /// LoadConnectionConfiguration fallback regardless of target framework. Only the WCF endpoint-behavior wiring
    /// below that reads it is net6.0+/net10.0-only, because on net48 the WcfSoapWithOpenIdConnect SOAP channels are
    /// built with ChannelFactory.CreateChannelWithIssuedToken(...), which does not go through EndpointBehaviors/
    /// IClientMessageInspector at all - see IshSession.CreateInfoShareWcfSoapWithOpenIdConnectConnection for the
    /// resulting net48 limitation (SOAP calls cannot carry the fallback header; a Write-Warning is emitted instead).
    /// </remarks>
    internal sealed class UserAgentState
    {
        /// <summary>
        /// Null (so header omitted, today's default behavior) until a 403 is observed against a User-Agent-sensitive
        /// WAF rule, at which point this becomes the RFC-sanctioned crawler self-identification convention
        /// (same format Googlebot/Bingbot use) for the remaining lifetime of the owning <see cref="Objects.Public.IshSession"/>.
        /// </summary>
        public string Value { get; set; }
    }

#if !NET48
    /// <summary>
    /// Sets the outgoing SOAP message's HTTP <c>User-Agent</c> header whenever <see cref="UserAgentState.Value"/> is
    /// non-null. Added as a <see cref="System.ServiceModel.Dispatcher.IClientMessageInspector"/> next to the existing
    /// <c>bearerCredentials</c> endpoint behavior wiring in <see cref="InfoShareWcfSoapWithOpenIdConnectConnection"/>.
    /// See #275.
    /// </summary>
    internal sealed class InfoShareWcfSoapUserAgentClientMessageInspector : IClientMessageInspector
    {
        private readonly UserAgentState _userAgentState;

        internal InfoShareWcfSoapUserAgentClientMessageInspector(UserAgentState userAgentState)
        {
            _userAgentState = userAgentState;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            if (_userAgentState.Value != null)
            {
                object propertyObject;
                HttpRequestMessageProperty httpRequestMessageProperty;
                if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out propertyObject))
                {
                    httpRequestMessageProperty = (HttpRequestMessageProperty)propertyObject;
                }
                else
                {
                    httpRequestMessageProperty = new HttpRequestMessageProperty();
                    request.Properties[HttpRequestMessageProperty.Name] = httpRequestMessageProperty;
                }
                httpRequestMessageProperty.Headers[HttpRequestHeader.UserAgent] = _userAgentState.Value;
            }
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // no-op, only outgoing requests need the User-Agent header
        }
    }

    /// <summary>
    /// Standard passthrough <see cref="IEndpointBehavior"/> wiring <see cref="InfoShareWcfSoapUserAgentClientMessageInspector"/>
    /// into a WCF client channel's <see cref="ClientRuntime"/>. Constructed once per <see cref="Objects.Public.IshSession"/>/
    /// <see cref="InfoShareWcfSoapWithOpenIdConnectConnection"/> holding a reference to the same <see cref="UserAgentState"/>
    /// so a later flip is visible to every channel immediately. See #275.
    /// </summary>
    internal sealed class InfoShareWcfSoapUserAgentEndpointBehavior : IEndpointBehavior
    {
        private readonly UserAgentState _userAgentState;

        internal InfoShareWcfSoapUserAgentEndpointBehavior(UserAgentState userAgentState)
        {
            _userAgentState = userAgentState;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // no-op
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new InfoShareWcfSoapUserAgentClientMessageInspector(_userAgentState));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // no-op, client-side only
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // no-op
        }
    }
#endif
}
