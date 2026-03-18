using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class WorkerPageContainerView : ContentView
    {
        public Grid ContainerGrid => WorkerPage_Container;
        public Border BtnWorkerBack => btn_worker_back;
        public VerticalStackLayout BtnWorkercategorysearch => btn_workercategorysearch;
        public VerticalStackLayout BtnWorkernamesearch => btn_workernamesearch;
        public VerticalStackLayout BtnWorkerbuildingsearch => btn_workerbuildingsearch;
        public Grid EntryWorkersearchContainer => entry_workersearch_container;
        public Label LbWorkerbuildingsearche => lb_workerbuildingsearche;
        public CustomEntry EntryWorkersearch => entry_workersearch;
        public ScrollView ListWorkerScroll => list_worker_scroll;
        public StackLayout ListWorker => list_worker;



        // WorkerPage Buttons


        public WorkerPageContainerView()
        {
            InitializeComponent();
            btn_workercategorysearch_img.Source = AppModel.Instance.imagesBase.Tools;
            btn_workernamesearch_img.Source = AppModel.Instance.imagesBase.Worker;
            btn_workerbuildingsearch_img.Source = AppModel.Instance.imagesBase.Building;
            btn_worker_back_img.Source = AppModel.Instance.imagesBase.DropLeftBlueDoubleImage;
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
