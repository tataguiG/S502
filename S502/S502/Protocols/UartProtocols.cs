using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S502.Protocols
{
    public class UartProtocols
    {
        public enum InstructionType
        {
            PowerOff = 0,
            Query = 1,
            Setting = 2,
            MarkerStimulate = 3
        }

        public CommProtocol GetProtocol(InstructionType type)
        {
            return _communicationProtocols[type];
        }

        private readonly Dictionary<InstructionType, CommProtocol> _communicationProtocols =
            new Dictionary<InstructionType, CommProtocol>();

        private void InitializeProtocols()
        {
            var protocol1 = new CommProtocol();
            var request = new RequestItem(new byte[2] { 0x20, 0x58 }, true);
            protocol1.AddRequestWithDefaultOperation(request);

            _communicationProtocols.Add(InstructionType.PowerOff, protocol1);

            var protocol2 = new CommProtocol();
            request = new RequestItem(new byte[2] { 0x20, 0x59 }, true);

            protocol2.AddRequestWithDefaultOperation(request);

            _communicationProtocols.Add(InstructionType.Query, protocol2);


            var protocol3 = new CommProtocol();
            request = new RequestItem(new byte[2] { 0x20, 0x5B }, true);
            protocol3.AddRequestWithDefaultOperation(request);
            _communicationProtocols.Add(InstructionType.MarkerStimulate, protocol3);

        }

        public CommProtocol BuildProtocol(byte[] detail)
        {
            if (IsValidDetail(detail))
            {
                var protocol4 = new CommProtocol();
                var request = new RequestItem(detail, true);
                protocol4.AddRequestWithDefaultOperation(request);
                _communicationProtocols.Add(InstructionType.Setting, protocol4);
                return protocol4;
            }
            return null;
        }

        private bool IsValidDetail(byte[] detail)
        {
            if (detail.Length != 5)
                return false;
            return true;
        }
    }
}
