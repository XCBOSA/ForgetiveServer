using Forgetive.Server.Math;
using static Forgetive.Server.ForgetiveServer;

namespace Forgetive.Server.Maps.NPCs
{
    /// <summary>
    /// 表示关于一个NPC的服务器向客户端发送的消息体
    /// </summary>
    public struct NPCServerToClientData
    {
        public NPCServerToClientData(string fromData)
        {
            string[] data = fromData.Split('@');
            NPCId = int.Parse(data[0]);
            Location = new Vector3(data[1]);
            Rotation = new Vector3(data[2]);
            NPCName = Base64Decode(data[3]);
            NPCSpeed = float.Parse(data[4]);
            NPCDamage = float.Parse(data[5]);
            IsAttacking = int.Parse(data[6]) == 1;
            ControllerName = Base64Decode(data[7]);
            IsAttackByOther = int.Parse(data[8]) == 1;
        }

        public NPCServerToClientData(int id, Vector3 location,
            Vector3 rotation, string name, string host, float speed,
            float npcDamage, bool isAttacking, bool isAttackByOther)
        {
            NPCId = id;
            Location = location;
            Rotation = rotation;
            NPCName = name;
            NPCSpeed = speed;
            NPCDamage = npcDamage;
            IsAttacking = isAttacking;
            ControllerName = host;
            IsAttackByOther = isAttackByOther;
        }

        public int NPCId;
        public Vector3 Location, Rotation;
        public string NPCName;
        public float NPCSpeed;
        public float NPCDamage;
        public bool IsAttacking, IsAttackByOther;
        public string ControllerName;

        public string ToData()
        {
            int i = 0;
            if (IsAttacking) i = 1;
            int j = 0;
            if (IsAttackByOther) j = 1;
            return NPCId + "@" + Location.ToXYZ() + "@" + Rotation.ToXYZ()
                + "@" + Base64Encode(NPCName) + "@" + NPCSpeed + "@" + NPCDamage +
                "@" + i + "@" + Base64Encode(ControllerName) + "@" + j;
        }
    }

    /// <summary>
    /// 表示关于一个NPC的宿主客户端向服务器发送的消息体
    /// </summary>
    public struct NPCHostToServerData
    {
        public NPCHostToServerData(string fromData)
        {
            string[] data = fromData.Split('@');
            NPCId = int.Parse(data[0]);
            Location = new Vector3(data[1]);
            Rotation = new Vector3(data[2]);
            IsAttacking = int.Parse(data[3]) == 1;
        }

        public NPCHostToServerData(int id, Vector3 location, Vector3 rotation, bool isAttacking)
        {
            NPCId = id;
            Location = location;
            Rotation = rotation;
            IsAttacking = isAttacking;
        }

        public int NPCId;
        public Vector3 Location, Rotation;
        public bool IsAttacking;

        public string ToData()
        {
            int i = 0;
            if (IsAttacking) i = 1;
            return NPCId + "@" + Location.ToXYZ() + "@" + Rotation.ToXYZ() + "@" + i;
        }
    }
}
