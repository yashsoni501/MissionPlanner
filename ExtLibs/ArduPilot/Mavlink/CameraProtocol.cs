using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MissionPlanner.ArduPilot.Mavlink
{
    public class CameraProtocol
    {
        private MAVState parent;
        private MAVLink.mavlink_camera_information_t info;
        private KeyValuePair<MAVLink.MAVLINK_MSG_ID, Func<MAVLink.MAVLinkMessage, bool>> sub;

        public async Task StartID(MAVState mavState)
        {
            parent = mavState;

            sub = mavState.parent.SubscribeToPacketType(MAVLink.MAVLINK_MSG_ID.CAMERA_INFORMATION, async message =>
                {
                    // not us
                    if (message.sysid != parent.sysid || message.compid != parent.compid)
                        return true;

                    info = ((MAVLink.mavlink_camera_information_t) message.data);

                    if (ASCIIEncoding.ASCII.GetString(info.cam_definition_uri) != "")
                    {
                        // get the uri
                    }

                    //if ((info.flags & (int) MAVLink.CAMERA_CAP_FLAGS.HAS_MODES) > 0)
                    {
                        if ((info.flags & (int) MAVLink.CAMERA_CAP_FLAGS.CAPTURE_IMAGE) > 0)
                            await parent.parent.doCommandAsync(parent.sysid, parent.compid,
                                MAVLink.MAV_CMD.REQUEST_CAMERA_INFORMATION, 0, 0, 0, 0, 0, 0, 0);
                        if ((info.flags & (int) MAVLink.CAMERA_CAP_FLAGS.CAPTURE_VIDEO) > 0)
                            await parent.parent.doCommandAsync(parent.sysid, parent.compid,
                                MAVLink.MAV_CMD.REQUEST_VIDEO_STREAM_INFORMATION, 0, 0, 0, 0, 0, 0, 0);
                        if ((info.flags & (int) MAVLink.CAMERA_CAP_FLAGS.CAPTURE_VIDEO) > 0)
                            await parent.parent.doCommandAsync(parent.sysid, parent.compid,
                                MAVLink.MAV_CMD.REQUEST_STORAGE_INFORMATION, 0, 0, 0, 0, 0, 0, 0);
                    }

                    return true;
                });

            var resp = await parent.parent.doCommandAsync(parent.sysid, parent.compid,
                MAVLink.MAV_CMD.REQUEST_CAMERA_INFORMATION, 0, 0, 0, 0, 0, 0, 0);
            if (resp)
            {
                // no use
            }
        }

        public MAVLink.CAMERA_CAP_FLAGS GetCameraModes()
        {
            return (MAVLink.CAMERA_CAP_FLAGS) info.flags;
        }
    }
}
