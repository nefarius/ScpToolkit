using System.Collections.Generic;
using System.ServiceModel;
using ScpControl.ScpCore;

namespace ScpControl.Wcf
{
    [ServiceContract]
    public interface IScpCommandService
    {
        [OperationContract]
        bool IsNativeFeedAvailable();

        [OperationContract]
        string GetActiveProfile();

        [OperationContract]
        string GetXml();

        [OperationContract]
        void SetXml(string xml);

        [OperationContract]
        void SetActiveProfile(Profile profile);

        [OperationContract]
        DsDetail GetPadDetail(DsPadId pad);

        [OperationContract]
        bool Rumble(DsPadId pad, byte large, byte small);

        [OperationContract]
        IEnumerable<string> GetProfileList();

        [OperationContract]
        GlobalConfiguration RequestConfiguration();

        [OperationContract]
        void SubmitConfiguration(GlobalConfiguration configuration);

        [OperationContract]
        IEnumerable<string> GetStatusData();

        [OperationContract]
        void PromotePad(byte pad);
    }
}