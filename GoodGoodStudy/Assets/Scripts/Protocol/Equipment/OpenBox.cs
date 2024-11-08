namespace Protocol
{
    public class C2SOpenBox : Message
    {
        public int OpenCount;
    }

    public class S2COpenBox : Message
    {
        public Equpment[] OpenedEquipments;
    }
}