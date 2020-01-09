using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using FclEx.Helpers;

namespace FclEx.Http.Core.Cookies
{
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal sealed class CookieInternal
    {
        internal const int MaxSupportedVersion = 1;
        internal const string CommentAttributeName = "Comment";
        internal const string CommentUrlAttributeName = "CommentURL";
        internal const string DiscardAttributeName = "Discard";
        internal const string DomainAttributeName = "Domain";
        internal const string ExpiresAttributeName = "Expires";
        internal const string MaxAgeAttributeName = "Max-Age";
        internal const string PathAttributeName = "Path";
        internal const string PortAttributeName = "Port";
        internal const string SecureAttributeName = "Secure";
        internal const string VersionAttributeName = "Version";
        internal const string HttpOnlyAttributeName = "HttpOnly";

        internal const string SeparatorLiteral = "; ";
        internal const string EqualsLiteral = "=";
        internal const string QuotesLiteral = "\"";
        internal const string SpecialAttributeLiteral = "$";

        internal static readonly char[] PortSplitDelimiters = new char[] { ' ', ',', '\"' };
        internal static readonly char[] Reserved2Name = new char[] { ' ', '\t', '\r', '\n', '=', ';', ',' };
        internal static readonly char[] Reserved2Value = new char[] { ';', ',' };

        // fields

        string m_comment = string.Empty;
        Uri m_commentUri = null;
        CookieVariant m_cookieVariant = CookieVariant.Plain;
        bool m_discard = false;
        string m_domain = string.Empty;
        bool m_domain_implicit = true;
        DateTime m_expires = DateTime.MinValue;
        string m_name = string.Empty;
        string m_path = string.Empty;
        bool m_path_implicit = true;
        string m_port = string.Empty;
        bool m_port_implicit = true;
        int[] m_port_list = null;
        bool m_secure = false;
        bool m_httpOnly = false;
        DateTime m_timeStamp = DateTime.Now;
        string m_value = string.Empty;
        int m_version = 0;

        string m_domainKey = string.Empty;
        internal bool IsQuotedVersion = false;
        internal bool IsQuotedDomain = false;


        // constructors

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CookieInternal()
        {
        }


        //public Cookie(string cookie) {
        //    if ((cookie == null) || (cookie == String.Empty)) {
        //        throw new ArgumentException("cookie");
        //    }
        //    Parse(cookie.Trim());
        //    Validate();
        //}

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CookieInternal(string name, string value)
        {
            Name = name;
            m_value = value;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CookieInternal(string name, string value, string path)
            : this(name, value)
        {
            Path = path;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CookieInternal(string name, string value, string path, string domain)
            : this(name, value, path)
        {
            Domain = domain;
        }

        // properties

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Comment
        {
            get => m_comment;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                m_comment = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Uri CommentUri
        {
            get => m_commentUri;
            set => m_commentUri = value;
        }


        public bool HttpOnly
        {
            get => m_httpOnly;
            set => m_httpOnly = value;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Discard
        {
            get => m_discard;
            set => m_discard = value;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Domain
        {
            get => m_domain;
            set
            {
                m_domain = value ?? String.Empty;
                m_domain_implicit = false;
                m_domainKey = string.Empty; //this will get it value when adding into the Container.
            }
        }

        private string _Domain => (Plain || m_domain_implicit || (m_domain.Length == 0))
            ? string.Empty
            : (SpecialAttributeLiteral
               + DomainAttributeName
               + EqualsLiteral + (IsQuotedDomain ? "\"" : string.Empty)
               + m_domain + (IsQuotedDomain ? "\"" : string.Empty)
            );

        internal bool DomainImplicit
        {
            get => m_domain_implicit;
            set => m_domain_implicit = value;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Expired
        {
            get
            {
                return (m_expires != DateTime.MinValue) && (m_expires.ToLocalTime() <= DateTime.Now);
            }
            set
            {
                if (value == true)
                {
                    m_expires = DateTime.Now;
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DateTime Expires
        {
            get => m_expires;
            set => m_expires = value;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (ValidationHelper.IsBlankString(value) || !InternalSetName(value))
                {
                    // throw new CookieException(SR.GetString(SR.net_cookie_attribute, "Name", value == null ? "<null>" : value));
                    throw new CookieException();
                }
            }
        }

        internal bool InternalSetName(string value)
        {
            if (ValidationHelper.IsBlankString(value) || value[0] == '$' || value.IndexOfAny(Reserved2Name) != -1)
            {
                m_name = string.Empty;
                return false;
            }
            m_name = value;
            return true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Path
        {
            get => m_path;
            set
            {
                m_path = (value == null ? String.Empty : value);
                m_path_implicit = false;
            }
        }

        private string _Path => (Plain || m_path_implicit || (m_path.Length == 0))
            ? string.Empty
            : (SpecialAttributeLiteral
               + PathAttributeName
               + EqualsLiteral
               + m_path
            );

        internal bool Plain => Variant == CookieVariant.Plain;

        internal CookieInternal Clone()
        {
            CookieInternal clonedCookie = new CookieInternal(m_name, m_value);

            //
            // Copy over all the properties from the original cookie
            //
            if (!m_port_implicit)
            {
                clonedCookie.Port = m_port;
            }
            if (!m_path_implicit)
            {
                clonedCookie.Path = m_path;
            }
            clonedCookie.Domain = m_domain;
            //
            // If the domain in the original cookie was implicit, we should preserve that property
            clonedCookie.DomainImplicit = m_domain_implicit;
            clonedCookie.m_timeStamp = m_timeStamp;
            clonedCookie.Comment = m_comment;
            clonedCookie.CommentUri = m_commentUri;
            clonedCookie.HttpOnly = m_httpOnly;
            clonedCookie.Discard = m_discard;
            clonedCookie.Expires = m_expires;
            clonedCookie.Version = m_version;
            clonedCookie.Secure = m_secure;

            //
            // The variant is set when we set properties like port/version. So, 
            // we should copy over the variant from the original cookie after 
            // we set all other properties
            clonedCookie.m_cookieVariant = m_cookieVariant;

            return clonedCookie;
        }

        private static bool IsDomainEqualToHost(string domain, string host)
        {
            //
            // +1 in the host length is to account for the leading dot in domain
            if ((host.Length + 1 == domain.Length) &&
                (string.Compare(host, 0, domain, 1, host.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //
        // According to spec we must assume default values for attributes but still
        // keep in mind that we must not include them into the requests.
        // We also check the validiy of all attributes based on the version and variant (read RFC)
        //
        // To work properly this function must be called after cookie construction with
        // default (response) URI AND set_default == true
        //
        // Afterwards, the function can be called many times with other URIs and
        // set_default == false to check whether this cookie matches given uri
        //

        internal bool VerifySetDefaults(CookieVariant variant, Uri uri, bool isLocalDomain, string localDomain, bool set_default, bool isThrow)
        {

            string host = uri.Host;
            int port = uri.Port;
            string path = uri.AbsolutePath;
            bool valid = true;

            if (set_default)
            {
                // Set Variant. If version is zero => reset cookie to Version0 style
                if (Version == 0)
                {
                    variant = CookieVariant.Plain;
                }
                else if (Version == 1 && variant == CookieVariant.Unknown)
                {
                    //since we don't expose Variant to an app, set it to Default
                    variant = CookieVariant.Default;
                }
                m_cookieVariant = variant;
            }

            //Check the name
            if (string.IsNullOrEmpty(m_name) || m_name[0] == '$' || m_name.IndexOfAny(Reserved2Name) != -1)
            {
                if (isThrow)
                {
                    // throw new CookieException(SR.GetString(SR.net_cookie_attribute, "Name", m_name == null ? "<null>" : m_name));
                    throw new CookieException();
                }
                return false;
            }

            //Check the value
            if (m_value == null ||
                (!(m_value.Length > 2 && m_value[0] == '\"' && m_value[m_value.Length - 1] == '\"') && m_value.IndexOfAny(Reserved2Value) != -1))
            {
                if (isThrow)
                {
                    // throw new CookieException(SR.GetString(SR.net_cookie_attribute, "Value", m_value == null ? "<null>" : m_value));
                    throw new CookieException();
                }
                return false;
            }

            //Check Comment syntax
            if (Comment != null && !(Comment.Length > 2 && Comment[0] == '\"' && Comment[Comment.Length - 1] == '\"')
                && (Comment.IndexOfAny(Reserved2Value) != -1))
            {
                if (isThrow)
                {
                    // throw new CookieException(SR.GetString(SR.net_cookie_attribute, CommentAttributeName, Comment));
                    throw new CookieException();
                }

                return false;
            }

            //Check Path syntax
            if (Path != null && !(Path.Length > 2 && Path[0] == '\"' && Path[Path.Length - 1] == '\"')
                && (Path.IndexOfAny(Reserved2Value) != -1))
            {
                if (isThrow)
                {
                    // throw new CookieException(SR.GetString(SR.net_cookie_attribute, PathAttributeName, Path));
                    throw new CookieException();
                }
                return false;
            }

            //Check/set domain
            // if domain is implicit => assume a) uri is valid, b) just set domain to uri hostname
            if (set_default && m_domain_implicit == true)
            {
                m_domain = host;
            }
            else
            {
                if (!m_domain_implicit)
                {
                    // Forwarding note: If Uri.Host is of IP address form then the only supported case
                    // is for IMPLICIT domain property of a cookie.
                    // The below code (explicit cookie.Domain value) will try to parse Uri.Host IP string
                    // as a fqdn and reject the cookie

                    //aliasing since we might need the KeyValue (but not the original one)
                    string domain = m_domain;

                    //Syntax check for Domain charset plus empty string
                    if (!DomainCharsTest(domain))
                    {
                        if (isThrow)
                        {
                            // throw new CookieException(SR.GetString(SR.net_cookie_attribute, DomainAttributeName, domain == null ? "<null>" : domain));
                            throw new CookieException();
                        }
                        return false;
                    }

                    //domain must start with '.' if set explicitly
                    if (domain[0] != '.')
                    {
                        if (!(variant == CookieVariant.Rfc2965 || variant == CookieVariant.Plain))
                        {
                            if (isThrow)
                            {
                                // throw new CookieException(SR.GetString(SR.net_cookie_attribute, DomainAttributeName, m_domain));
                                throw new CookieException();
                            }
                            return false;
                        }
                        domain = '.' + domain;
                    }

                    int host_dot = host.IndexOf('.');

                    // First quick check is for pushing a cookie into the local domain
                    if (isLocalDomain && (string.Compare(localDomain, domain, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        valid = true;
                    }
                    else if (domain.IndexOf('.', 1, domain.Length - 2) == -1)
                    {

                        // A single label domain is valid only if the domain is exactly the same as the host specified in the URI
                        if (!IsDomainEqualToHost(domain, host))
                        {
                            valid = false;
                        }
                    }
                    else if (variant == CookieVariant.Plain)
                    {
                        // We distiguish between Version0 cookie and other versions on domain issue
                        // According to Version0 spec a domain must be just a substring of the hostname

                        if (!IsDomainEqualToHost(domain, host))
                        {
                            if (host.Length <= domain.Length ||
                                string.Compare(host, host.Length - domain.Length, domain, 0, domain.Length, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                valid = false;
                            }
                        }
                    }
                    else if (host_dot == -1 ||
                             domain.Length != host.Length - host_dot ||
                             string.Compare(host, host_dot, domain, 0, domain.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        //starting the first dot, the host must match the domain

                        // for null hosts, the host must match the domain exactly
                        if (!IsDomainEqualToHost(domain, host))
                        {
                            valid = false;
                        }
                    }

                    if (valid)
                    {
                        // m_domainKey = domain.ToLower(CultureInfo.InvariantCulture);
                        m_domainKey = domain.ToLowerInvariant();
                    }
                }
                else
                {
                    // for implicitly set domain AND at the set_default==false time
                    // we simply got to match uri.Host against m_domain
                    if (string.Compare(host, m_domain, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        valid = false;
                    }

                }
                if (!valid)
                {
                    if (isThrow)
                    {
                        // throw new CookieException(SR.GetString(SR.net_cookie_attribute, DomainAttributeName, m_domain));
                        throw new CookieException();
                    }
                    return false;
                }
            }


            //Check/Set Path

            if (set_default && m_path_implicit == true)
            {
                //assuming that uri path is always valid and contains at least one '/'
                switch (m_cookieVariant)
                {
                    case CookieVariant.Plain:
                        m_path = path;
                        break;
                    case CookieVariant.Rfc2109:
                        m_path = path.Substring(0, path.LastIndexOf('/')); //may be empty
                        break;

                    case CookieVariant.Rfc2965:
                    default:                //hope future versions will have same 'Path' semantic?
                        m_path = path.Substring(0, path.LastIndexOf('/') + 1);
                        break;

                }
            }
            else
            {
                //check current path (implicit/explicit) against given uri
                if (!path.StartsWith(CookieParser.CheckQuoted(m_path)))
                {
                    if (isThrow)
                    {
                        // throw new CookieException(SR.GetString(SR.net_cookie_attribute, PathAttributeName, m_path));
                        throw new CookieException();
                    }
                    return false;
                }
            }

            // set the default port if Port attribute was present but had no value
            if (set_default && (m_port_implicit == false && m_port.Length == 0))
            {
                m_port_list = new int[1] { port };
            }

            if (m_port_implicit == false)
            {
                // Port must match agaist the one from the uri
                valid = false;
                foreach (int p in m_port_list)
                {
                    if (p == port)
                    {
                        valid = true;
                        break;
                    }
                }
                if (!valid)
                {
                    if (isThrow)
                    {
                        // throw new CookieException(SR.GetString(SR.net_cookie_attribute, PortAttributeName, m_port));
                        throw new CookieException();
                    }
                    return false;
                }
            }
            return true;
        }

        //Very primitive test to make sure that the name does not have illegal characters
        // As per RFC 952 (relaxed on first char could be a digit and string can have '_')
        private static bool DomainCharsTest(string name)
        {
            if (name == null || name.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < name.Length; ++i)
            {
                char ch = name[i];
                if (!(
                    (ch >= '0' && ch <= '9') ||
                    (ch == '.' || ch == '-') ||
                    (ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch == '_')
                ))
                    return false;
            }
            return true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Port
        {
            get => m_port;
            set
            {
                m_port_implicit = false;
                if (string.IsNullOrEmpty(value))
                {
                    //"Port" is present but has no value.
                    m_port = string.Empty;
                }
                else
                {
                    // Parse port list
                    if (value[0] != '\"' || value[value.Length - 1] != '\"')
                    {

                        // throw new CookieException($"{PortAttributeName}: {value}");
                        throw new CookieException();
                    }
                    string[] ports = value.Split(PortSplitDelimiters);

                    List<int> portList = new List<int>();
                    for (int i = 0; i < ports.Length; ++i)
                    {
                        // Skip spaces
                        if (ports[i] != string.Empty)
                        {
                            if (!Int32.TryParse(ports[i], out int port))
                                // throw new CookieException($"{PortAttributeName}: {value}");
                                throw new CookieException();

                            // valid values for port 0 - 0xFFFF
                            if ((port < 0) || (port > 0xFFFF))
                                // throw new CookieException($"{PortAttributeName}: {value}");
                                throw new CookieException();

                            portList.Add(port);
                        }
                    }
                    m_port_list = portList.ToArray();
                    m_port = value;
                    m_version = MaxSupportedVersion;
                    m_cookieVariant = CookieVariant.Rfc2965;
                }
            }
        }


        internal int[] PortList => m_port_list;

        private string _Port => m_port_implicit ? string.Empty :
            (SpecialAttributeLiteral
             + PortAttributeName
             + ((m_port.Length == 0) ? string.Empty : (EqualsLiteral + m_port))
            );

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Secure
        {
            get => m_secure;
            set => m_secure = value;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DateTime TimeStamp => m_timeStamp;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Value
        {
            get => m_value;
            set => m_value = (value == null ? String.Empty : value);
        }

        internal CookieVariant Variant
        {
            get => m_cookieVariant;
            set
            {
                // only set by HttpListenerRequest::Cookies_get()
                Debug.Assert(value == CookieVariant.Rfc2965, "Cookie#{0}::set_Variant()|value:{1}", ValidationHelper.HashString(this), value);
                m_cookieVariant = value;
            }
        }

        // m_domainKey member is set internally in VerifySetDefaults()
        // If it is not set then verification function was not called
        // and this should never happen
        internal string DomainKey => m_domain_implicit ? Domain : m_domainKey;


        //public Version Version {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Version
        {
            get => m_version;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_version = value;
                if (value > 0 && m_cookieVariant < CookieVariant.Rfc2109)
                {
                    m_cookieVariant = CookieVariant.Rfc2109;
                }
            }
        }

        private string _Version => (Version == 0) ? string.Empty :
            (SpecialAttributeLiteral
             + VersionAttributeName
             + EqualsLiteral + (IsQuotedVersion ? "\"" : string.Empty)
             + m_version.ToString(NumberFormatInfo.InvariantInfo) + (IsQuotedVersion ? "\"" : string.Empty));

        // methods


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool Equals(object comparand)
        {
            if (!(comparand is CookieInternal))
            {
                return false;
            }

            CookieInternal other = (CookieInternal)comparand;

            return (string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase) == 0)
                   && (string.Compare(Value, other.Value, StringComparison.Ordinal) == 0)
                   && (string.Compare(Path, other.Path, StringComparison.Ordinal) == 0)
                   && (string.Compare(Domain, other.Domain, StringComparison.OrdinalIgnoreCase) == 0)
                   && (Version == other.Version)
                ;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode()
        {
            //
            //string hashString = Name + "=" + Value + ";" + Path + "; " + Domain + "; " + Version;
            //int hash = 0;
            //
            //foreach (char ch in hashString) {
            //    hash = unchecked(hash << 1 ^ (int)ch);
            //}
            //return hash;
            return (Name + "=" + Value + ";" + Path + "; " + Domain + "; " + Version).GetHashCode();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string ToString()
        {

            string domain = _Domain;
            string path = _Path;
            string port = _Port;
            string version = _Version;

            string result =
                    ((version.Length == 0) ? string.Empty : (version + SeparatorLiteral))
                    + Name + EqualsLiteral + Value
                    + ((path.Length == 0) ? string.Empty : (SeparatorLiteral + path))
                    + ((domain.Length == 0) ? string.Empty : (SeparatorLiteral + domain))
                    + ((port.Length == 0) ? string.Empty : (SeparatorLiteral + port))
                ;
            if (result == "=")
            {
                return string.Empty;
            }
            return result;
        }

        internal string ToServerString()
        {
            string result = Name + EqualsLiteral + Value;
            if (!string.IsNullOrEmpty(m_comment))
            {
                result += SeparatorLiteral + CommentAttributeName + EqualsLiteral + m_comment;
            }
            if (m_commentUri != null)
            {
                result += SeparatorLiteral + CommentUrlAttributeName + EqualsLiteral + QuotesLiteral + m_commentUri.ToString() + QuotesLiteral;
            }
            if (m_discard)
            {
                result += SeparatorLiteral + DiscardAttributeName;
            }
            if (!m_domain_implicit && !string.IsNullOrEmpty(m_domain))
            {
                result += SeparatorLiteral + DomainAttributeName + EqualsLiteral + m_domain;
            }
            if (Expires != DateTime.MinValue)
            {
                int seconds = (int)(Expires.ToLocalTime() - DateTime.Now).TotalSeconds;
                if (seconds < 0)
                {
                    // This means that the cookie has already expired. Set Max-Age to 0
                    // so that the client will discard the cookie immediately
                    seconds = 0;
                }
                result += SeparatorLiteral + MaxAgeAttributeName + EqualsLiteral + seconds.ToString(NumberFormatInfo.InvariantInfo);
            }
            if (!m_path_implicit && !string.IsNullOrEmpty(m_path))
            {
                result += SeparatorLiteral + PathAttributeName + EqualsLiteral + m_path;
            }
            if (!Plain && !m_port_implicit && !string.IsNullOrEmpty(m_port))
            {
                // QuotesLiteral are included in m_port
                result += SeparatorLiteral + PortAttributeName + EqualsLiteral + m_port;
            }
            if (m_version > 0)
            {
                result += SeparatorLiteral + VersionAttributeName + EqualsLiteral + m_version.ToString(NumberFormatInfo.InvariantInfo);
            }
            return result == EqualsLiteral ? null : result;
        }

        //private void Validate() {
        //    if ((m_name == String.Empty) && (m_value == String.Empty)) {
        //        throw new CookieException();
        //    }
        //    if ((m_name != String.Empty) && (m_name[0] == '$')) {
        //        throw new CookieException();
        //    }
        //}

#if DEBUG
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal void Dump()
        {
            DebuggerHepler.Write("Cookie: " + ToString() + "->\n"
                            + "\tComment    = " + Comment + "\n"
                            + "\tCommentUri = " + CommentUri + "\n"
                            + "\tDiscard    = " + Discard + "\n"
                            + "\tDomain     = " + Domain + "\n"
                            + "\tExpired    = " + Expired + "\n"
                            + "\tExpires    = " + Expires + "\n"
                            + "\tName       = " + Name + "\n"
                            + "\tPath       = " + Path + "\n"
                            + "\tPort       = " + Port + "\n"
                            + "\tSecure     = " + Secure + "\n"
                            + "\tTimeStamp  = " + TimeStamp + "\n"
                            + "\tValue      = " + Value + "\n"
                            + "\tVariant    = " + Variant + "\n"
                            + "\tVersion    = " + Version + "\n"
                            + "\tHttpOnly    = " + HttpOnly + "\n"
            );
        }
#endif

        public Cookie ToCookie()
        {
            var cookie = new Cookie(Name, Value, Path)
            {
                Comment = Comment,
                CommentUri = CommentUri,
                Discard = Discard,
                Expired = Expired,
                Expires = Expires,
                HttpOnly = HttpOnly,
                Secure = Secure,
                // Version = Version,
            };

            if (!Domain.IsNullOrEmpty()) cookie.Domain = Domain;
            // if (!Port.IsNullOrEmpty()) cookie.Port = Port;
            return cookie;
        }
    }

}
