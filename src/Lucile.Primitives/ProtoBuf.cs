#pragma warning disable SA1402 // FileMayOnlyContainASingleClass
#pragma warning disable SA1649 // SA1649FileNameMustMatchTypeName

using System;

namespace ProtoBuf
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class ProtoContractAttribute : Attribute
    {
        private const byte OptionsAsReferenceDefault = 32;
        private const byte OptionsEnumPassthru = 64;
        private const byte OptionsEnumPassthruHasValue = 128;
        private const byte OptionsIgnoreListHandling = 16;
        private const byte OptionsInferTagFromName = 1;
        private const byte OptionsInferTagFromNameHasValue = 2;
        private const byte OptionsSkipConstructor = 8;
        private const byte OptionsUseProtoMembersOnly = 4;
        private int _dataMemberOffset;
        private byte _flags;
        private int _implicitFirstTag;
        private string _name;

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
                return this._dataMemberOffset;
            }

            set
            {
                this._dataMemberOffset = value;
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
        /// Gets or sets the fist offset to use with implicit field tags;
        /// only uesd if ImplicitFields is set.
        /// </summary>
        public int ImplicitFirstTag
        {
            get
            {
                return this._implicitFirstTag;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("ImplicitFirstTag");
                }

                this._implicitFirstTag = value;
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
        /// Gets or sets the defined name of the type.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                this._name = value;
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
        /// Has a EnumPassthru value been explicitly set?
        /// </summary>
        internal bool EnumPassthruHasValue
        {
            get
            {
                return this.HasFlag(128);
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

        private bool HasFlag(byte flag)
        {
            return (this._flags & flag) == flag;
        }

        private void SetFlag(byte flag, bool value)
        {
            if (value)
            {
                this._flags |= flag;
                return;
            }

            this._flags &= (byte)~flag;
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
        private readonly int _tag;

        private Type _knownType;

        /// <summary>
        ///  Creates a new instance of the ProtoIncludeAttribute.
        ///  </summary>
        ///  <param name="tag">The unique index (within the type) that will identify this data.</param>
        ///  <param name="knownType">The additional type to serialize/deserialize.</param>
        public ProtoIncludeAttribute(int tag, Type knownType)
        {
            this._tag = tag;
            this._knownType = knownType;
        }

        public Type KnownType
        {
            get { return _knownType; }
            set { _knownType = value; }
        }

        public int Tag
        {
            get
            {
                return this._tag;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal class ProtoMemberAttribute : Attribute
    {
        private bool _asReference;
        private bool _asReferenceHasValue;
        private string _name;
        private int _tag;

        public ProtoMemberAttribute(int tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Enables full object-tracking/full-graph support.
        /// </summary>
        public bool AsReference
        {
            get
            {
                return _asReference;
            }

            set
            {
                _asReferenceHasValue = true;
                this._asReference = value;
            }
        }

        public bool AsReferenceHasValue
        {
            get { return _asReferenceHasValue; }
        }

        /// <summary>
        /// Gets or sets the original name defined in the .proto; not used
        /// during serialization.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                this._name = value;
            }
        }

        /// <summary>
        /// Gets the unique tag used to identify this member within the type.
        /// </summary>
        public int Tag
        {
            get
            {
                return this._tag;
            }
        }
    }
}

#pragma warning restore SA1402 // FileMayOnlyContainASingleClass
#pragma warning restore SA1649 // SA1649FileNameMustMatchTypeName