using System.Threading.Tasks;
using FclEx.Http.Actions;

namespace FclEx.Http.Event
{
    // 之所以设置成返回task的形式是因为可以在调用的时候await
    public delegate ValueTask ActionEventListener(IAction sender, ActionEvent actionEvent);
}
