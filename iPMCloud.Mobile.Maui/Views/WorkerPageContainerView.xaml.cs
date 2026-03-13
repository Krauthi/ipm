using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class WorkerPageContainerView : ContentView
    {
        public Grid ContainerGrid => WorkerPage_Container;
        public Border BtnWorkerBack => btn_worker_back;
        public Image BtnWorkerBackImg => btn_worker_back_img;
        public VerticalStackLayout BtnWorkercategorysearch => btn_workercategorysearch;
        public Image BtnWorkercategorysearchImg => btn_workercategorysearch_img;
        public VerticalStackLayout BtnWorkernamesearch => btn_workernamesearch;
        public Image BtnWorkernamesearchImg => btn_workernamesearch_img;
        public VerticalStackLayout BtnWorkerbuildingsearch => btn_workerbuildingsearch;
        public Image BtnWorkerbuildingsearchImg => btn_workerbuildingsearch_img;
        public Grid EntryWorkersearchContainer => entry_workersearch_container;
        public Label LbWorkerbuildingsearche => lb_workerbuildingsearche;
        public CustomEntry EntryWorkersearch => entry_workersearch;
        public ScrollView ListWorkerScroll => list_worker_scroll;
        public StackLayout ListWorker => list_worker;

        public WorkerPageContainerView()
        {
            InitializeComponent();
        }

        private void Entry_workersearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (list_worker.Children.Count > 0)
            {
                foreach (var child in list_worker.Children)
                {
                    // Cast zu VisualElement (hat IsVisible und ClassId)
                    if (child is VisualElement element)
                    {
                        if (element.ClassId != null && element.ClassId.Length > 1 && element.ClassId.Substring(0, 2) == "##")
                        {
                            element.IsVisible = element.ClassId.ToLower().Contains(e.NewTextValue.ToLower());
                        }
                        else if (element.ClassId != null && element.ClassId.Length > 2 && element.ClassId.Substring(0, 3) == "bu_")
                        {
                            element.IsVisible = element.ClassId.ToLower().Contains(e.NewTextValue.ToLower());
                        }
                    }
                }
            }
        }
    }
}
