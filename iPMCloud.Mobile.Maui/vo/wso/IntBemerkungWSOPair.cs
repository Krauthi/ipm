using System;
using Xamarin.Forms;

namespace iPMCloud.Mobile.vo.wso
{
    public class IntBemerkungWSOPair
    {
        public Int32 id = -1;
        public BemerkungWSO bem = null;
        public LeistungWSO lei = null;
        public Label badge = null;
        public StackLayout badgeStack;
        public int count = 0;
    }
}
