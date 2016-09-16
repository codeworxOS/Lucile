using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoBuf
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class ProtoContractAttribute : Attribute
    {
        private const byte OPTIONS_InferTagFromName = 1;
        private const byte OPTIONS_InferTagFromNameHasValue = 2;
        private const byte OPTIONS_UseProtoMembersOnly = 4;
        private const byte OPTIONS_SkipConstructor = 8;
        private const byte OPTIONS_IgnoreListHandling = 16;
        private const byte OPTIONS_AsReferenceDefault = 32;
        private const byte OPTIONS_EnumPassthru = 64;
        private const byte OPTIONS_EnumPassthruHasValue = 128;
        private string name;
        private int implicitFirstTag;
        private int dataMemberOffset;
        private byte flags;
        /// <summary>
        /// Gets or sets the defined name of the type.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        /// <summary>
        /// Gets or sets the fist offset to use with implicit field tags;
        /// only uesd if ImplicitFields is set.
        /// </summary>
        public int ImplicitFirstTag
        {
            get
            {
                return this.implicitFirstTag;
            }
            set
            {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("ImplicitFirstTag");
                }
                this.implicitFirstTag = value;
            }
        }
        /// <summary>
        /// If specified, alternative contract markers (such as markers for XmlSerailizer or DataContractSerializer) are ignored.
        /// </summary>
        public bool UseProtoMembersOnly
        {
            get
            {
                return this.HasFlag(4);
            }
            set
            {
                this.SetFlag(4, value);
            }
        }
        /// <summary>
        /// If specified, do NOT treat this type as a list, even if it looks like one.
        /// </summary>
        public bool IgnoreListHandling
        {
            get
            {
                return this.HasFlag(16);
            }
            set
            {
                this.SetFlag(16, value);
            }
        }

        /// <summary>
        /// Enables/disables automatic tag generation based on the existing name / order
        /// of the defined members. This option is not used for members marked
        /// with ProtoMemberAttribute, as intended to provide compatibility with
        /// WCF serialization. WARNING: when adding new fields you must take
        /// care to increase the Order for new elements, otherwise data corruption
        /// may occur.
        /// </summary>
        /// <remarks>If not explicitly specified, the default is assumed from Serializer.GlobalOptions.InferTagFromName.</remarks>
        public bool InferTagFromName
        {
            get
            {
                return this.HasFlag(1);
            }
            set
            {
                this.SetFlag(1, value);
                this.SetFlag(2, true);
            }
        }
        /// <summary>
        /// Has a InferTagFromName value been explicitly set? if not, the default from the type-model is assumed.
        /// </summary>
        internal bool InferTagFromNameHasValue
        {
            get
            {
                return this.HasFlag(2);
            }
        }
        /// <summary>
        /// Specifies an offset to apply to [DataMember(Order=...)] markers;
        /// this is useful when working with mex-generated classes that have
        /// a different origin (usually 1 vs 0) than the original data-contract.
        ///
        /// This value is added to the Order of each member.
        /// </summary>
        public int DataMemberOffset
        {
            get
            {
                return this.dataMemberOffset;
            }
            set
            {
                this.dataMemberOffset = value;
            }
        }
        /// <summary>
        /// If true, the constructor for the type is bypassed during deserialization, meaning any field initializers
        /// or other initialization code is skipped.
        /// </summary>
        public bool SkipConstructor
        {
            get
            {
                return this.HasFlag(8);
            }
            set
            {
                this.SetFlag(8, value);
            }
        }
        /// <summary>
        /// Should this type be treated as a reference by default? Please also see the implications of this,
        /// as recorded on ProtoMemberAttribute.AsReference
        /// </summary>
        public bool AsReferenceDefault
        {
            get
            {
                return this.HasFlag(32);
            }
            set
            {
                this.SetFlag(32, value);
            }
        }
        /// <summary>
        /// Applies only to enums (not to DTO classes themselves); gets or sets a value indicating that an enum should be treated directly as an int/short/etc, rather
        /// than enforcing .proto enum rules. This is useful *in particul* for [Flags] enums.
        /// </summary>
        public bool EnumPassthru
        {
            get
            {
                return this.HasFlag(64);
            }
            set
            {
                this.SetFlag(64, value);
                this.SetFlag(128, true);
            }
        }
        /// <summary>
        /// Has a EnumPassthru value been explicitly set?
        /// </summary>
        internal bool EnumPassthruHasValue
        {
            get
            {
                return this.HasFlag(128);
            }
        }
        private bool HasFlag(byte flag)
        {
            return (this.flags & flag) == flag;
        }
        private void SetFlag(byte flag, bool value)
        {
            if (value) {
                this.flags |= flag;
                return;
            }
            this.flags &= (byte)~flag;
        }
    }

    /// <summary>
    /// Indicates the known-types to support for an individual
    /// message. This serializes each level in the hierarchy as
    /// a nested message to retain wire-compatibility with
    /// other protocol-buffer implementations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
    internal sealed class ProtoIncludeAttribute : Attribute
    {
        private readonly int tag;

        private Type knownType;

        public Type KnownType
        {
            get { return knownType; }
            set { knownType = value; }
        }


        public int Tag
        {
            get
            {
                return this.tag;
            }
        }

        /// <summary>
        ///  Creates a new instance of the ProtoIncludeAttribute.
        ///  </summary>
        ///  <param name="tag">The unique index (within the type) that will identify this data.</param>
        ///  <param name="knownType">The additional type to serialize/deserialize.</param>
        public ProtoIncludeAttribute(int tag, Type knownType)
        {
            this.tag = tag;
            this.knownType = knownType;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal class ProtoMemberAttribute : Attribute
    {
        private string name;
        private int tag;
        private bool asReferenceHasValue;
        private bool asReference;

        /// <summary>
        /// Gets or sets the original name defined in the .proto; not used
        /// during serialization.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        /// <summary>
        /// Gets the unique tag used to identify this member within the type.
        /// </summary>
        public int Tag
        {
            get
            {
                return this.tag;
            }
        }

        /// <summary>
        /// Enables full object-tracking/full-graph support.
        /// </summary>
        public bool AsReference
        {
            get { return asReference; }
            set
            {
                asReferenceHasValue = true;
                this.asReference = value;
            }
        }

        public bool AsReferenceHasValue
        {
            get { return asReferenceHasValue; }
        }

        public ProtoMemberAttribute(int tag)
        {
            this.tag = tag;
        }
    }
}
