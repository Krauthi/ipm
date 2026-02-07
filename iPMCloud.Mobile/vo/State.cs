using System;
using System.Collections.Generic;
using System.Text;

namespace iPMCloud.Mobile.vo
{
    public class State
    {
        AppModel model = null;
        public State(AppModel _model)
        {
            model = _model;
        }

        // Wenn wieder zurück zur Login Page und damit nicht das Autologin reagiert!
        public bool IsBackTappedToLogin { get; set; } = false;



    }
}
