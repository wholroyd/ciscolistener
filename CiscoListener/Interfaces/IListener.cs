using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CiscoListener.Interfaces
{
    [MatchAllEndpoints]
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IListener
    {
        [OperationContract(IsOneWay = false, Action = "*", ReplyAction = "*")]
        Message ProcessMessage(Message message);
    }
}