using Forgetive.Server.Math;
using System;

namespace Forgetive.Server.Maps.NPCs
{
    public class NPC
    {
        public int NPCId;
        public bool CanAttackPlayer;
        public int Damage;
        public Vector3 Location, Rotation;
        public Player Host;
        public string Name;
        public float NPCHealth;
        public float NPCMoveSpeed;
        public bool IsAttacking, IsAttackByOther;
        public DateTime NotAttackTime = DateTime.MaxValue,
            NotAttackByOtherTime = DateTime.MaxValue;

        public int NPCTakeItemId;
        public string NPCTakeItemDesc;
        public string NPCTakeItemExtra;

        public void Dispose(bool needDisposePlayerNPC)
        {
            if (Host != null && needDisposePlayerNPC)
            {
                for (int i = 0; i < Host.ownedNPCs.Count; i++)
                {
                    if (Host.ownedNPCs[i] == NPCId)
                    {
                        Host.ownedNPCs.RemoveAt(i);
                        Host.DisposeNPCHost(NPCId);
                        i--;
                    }
                }
            }
            for (int i = 0; i < NPCManager.NPCs.Count; i++)
            {
                if (NPCManager.NPCs[i].NPCId == NPCId)
                {
                    NPCManager.All_DisposeNPCMessage(NPCId);
                    NPCManager.NPCs.RemoveAt(i);
                    i--;
                }
            }
        }

        public NPCServerToClientData GetServerToClientData()
        {
            return new NPCServerToClientData(
                NPCId, Location, Rotation, Name, Host.Name,
                NPCMoveSpeed, Damage, IsAttacking, IsAttackByOther);
        }

        public void ApplyHostData(NPCHostToServerData data)
        {
            if (NPCId == data.NPCId)
            {
                Location = data.Location;
                Rotation = data.Rotation;
                if (data.IsAttacking)
                {
                    IsAttacking = true;
                    NotAttackTime = DateTime.Now + new TimeSpan(0, 0, 1);
                }
            }
        }

        public override string ToString()
        {
            return "NPC {Id=" + NPCId + "; CanAttack=" + CanAttackPlayer + "; Damage="
                + Damage + "; Location=" + Location + "; Rotation=" + Rotation + "; HostName="
                + Host.Name + "; Name=" + Name + "; Health=" + NPCHealth + "; Speed="
                + NPCMoveSpeed + "; TakeItem={StaticId=" + NPCTakeItemId + "; Desc="
                + NPCTakeItemDesc + "; Extra=" + NPCTakeItemExtra + "} }";
        }
    }
}
