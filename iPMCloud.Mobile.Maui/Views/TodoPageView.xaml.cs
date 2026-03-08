using iPMCloud.Mobile.vo;

namespace iPMCloud.Mobile.Views
{
    public partial class TodoPageView : ContentView
    {
        public TodoPageView()
        {
            InitializeComponent();
        }

        public Grid Container => TodoPage_Container;
        public void SetVisible(bool visible) => TodoPage_Container.IsVisible = visible;

        // Expose child elements so MainPage can access them after extraction
        public Border BtnTodoBack => btn_todo_back;
        public Image BtnTodoBackImg => btn_todo_back_img;
        public StackLayout BtnTodoFaellig => btn_todo_faellig;
        public StackLayout BtnTodoAll => btn_todo_all;
        public StackLayout BtnTodoInout => btn_todo_inout;
        public Image BtnTodoInout2Img => btn_todo_inout2_img;
        public Image BtnTodoInoutImg => btn_todo_inout_img;
        public Grid EntryTodosearchContainer => entry_todosearch_container;
        public Label EntryTodosearchLbb => entry_todosearch_lbb;
        public CustomEntry EntryTodosearch => entry_todosearch;
        public Image BtnTodosearchImg => btn_todosearch_img;
        public Grid EntryTodosearchStepcontainer => entry_todosearch_stepcontainer;
        public StackLayout BtnTodoFaelligPrev => btn_todo_faellig_prev;
        public Label BtnTodoFaelligCount => btn_todo_faellig_count;
        public StackLayout BtnTodoFaelligNext => btn_todo_faellig_next;
        public ScrollView ListTodoScroll => list_todo_scroll;
        public StackLayout ListTodo => list_todo;
    }
}
