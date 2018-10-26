
namespace com.perroelectrico.flip.controller {

    public interface IMenuController {
        string Id();
        void DoBeforeArrival();
        void DoAfterArrival();
        void DoOnLeaving();
        void Back();
        void Execute(string cmd);
    }
}