using System;
using System.Collections.Generic;
using System.Text;

namespace iPMCloud.Mobile.vo
{
    public class State
    {
        public State()
        {
        }

        // Wenn wieder zurück zur Login Page und damit nicht das Autologin reagiert!
        public bool IsBackTappedToLogin { get; set; } = false;



    }
}
