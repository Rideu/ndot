namespace ndot
{
    public enum DnsRequestType : ushort
    {
        /// <summary> Resolve 32-bit IP Address </summary>
        A = 1,
        /// <summary> Specifies the name of a DNS name server that is authoritative for the zone. Each zone must have at least one NS record that points to its primary name server, and that name must also have a valid A (Address) record </summary>
        NS = 2,
        /// <summary> The CNAME record provides a mapping between this alias and the “canonical” (real) name of the node. </summary> 
        CNAME = 5,
        /// <summary> Specifies the location (device name) that is responsible for handling e-mail sent to the domain. </summary>
        MX = 15,
        /// <summary> Allows arbitrary additional text associated with the domain to be stored.  </summary>
        TXT = 16,
        /// <summary> Returns a 128-bit IPv6 address, most commonly used to map hostnames to an IP address of the host.  </summary>
        AAAA = 28,
    }
}
