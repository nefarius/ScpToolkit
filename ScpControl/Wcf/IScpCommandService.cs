using System.Collections.Generic;
using System.ServiceModel;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using ScpControl.Shared.Core;

namespace ScpControl.Wcf
{
    [ServiceContract]
    public interface IScpCommandService
    {
        [OperationContract]
        bool IsNativeFeedAvailable();

        [OperationContract]
        DsDetail GetPadDetail(DsPadId pad);

        [OperationContract]
        bool Rumble(DsPadId pad, byte large, byte small);

        [OperationContract]
        GlobalConfiguration RequestConfiguration();

        [OperationContract]
        void SubmitConfiguration(GlobalConfiguration configuration);

        [OperationContract]
        IEnumerable<string> GetStatusData();

        [OperationContract]
        void PromotePad(byte pad);

        [OperationContract]
        IEnumerable<DualShockProfile> GetProfiles();

        [OperationContract]
        void SubmitProfile(DualShockProfile profile);

        [OperationContract]
        void RemoveProfile(DualShockProfile profile);
    }
}