using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// A generic base class for all ISH(Object) types holding IshFields. 
    /// Mainly to make WrapAsPSObjectAndAddNoteProperties(...) implementation easier.
    /// </summary>
    public abstract class IshBaseObject
    {
        /// <summary>
        /// Gets and sets the IshFields property.
        /// The IshFields property is a collection of <see cref="IshFields"/>.
        /// </summary>
        internal virtual IshFields IshFields { get; set; }
    }
}
