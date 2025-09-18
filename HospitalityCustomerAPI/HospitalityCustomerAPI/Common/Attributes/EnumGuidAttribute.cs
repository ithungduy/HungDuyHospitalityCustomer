namespace HospitalityCustomerAPI.Common.Attributes
{
    class EnumGuidAttribute : Attribute
    {
        public Guid Guid;

        public EnumGuidAttribute(string guid)
        {
            Guid = new Guid(guid);
        }
        public EnumGuidAttribute()
        {
            Guid = Utility.defaultUID;
        }
    }
}
