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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trisoft.ISHRemote.Exceptions
{
    public class TrisoftAutomationException : ApplicationException
    {
        private string _message;
        private string _frame;

        public TrisoftAutomationException(string message)
        {
            System.Diagnostics.StackFrame stackframe = new System.Diagnostics.StackFrame(1);
            _frame = stackframe.GetMethod().ToString();
            _message = message;
        }

        public override string ToString()
        {
            return _frame;
        }
    }
}
