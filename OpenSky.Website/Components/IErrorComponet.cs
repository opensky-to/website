using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSky.Website.Components
{
    public interface IErrorComponent
    {
        void ShowError(string title, string message);
    }
}
