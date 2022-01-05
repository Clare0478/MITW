using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace FHIR_Demo.Models
{
    public class MHB_Convert
    {

        // 注意: 產生的程式碼可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class myhealthbank
        {

            private myhealthbankBdata bdataField;

            /// <remarks/>
            public myhealthbankBdata bdata
            {
                get
                {
                    return this.bdataField;
                }
                set
                {
                    this.bdataField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdata
        {

            private string b11Field;

            private uint b12Field;

            private string r0Field;

            private myhealthbankBdataR1[] r1Field;

            private myhealthbankBdataR2[] r2Field;

            private myhealthbankBdataR3[] r3Field;

            private myhealthbankBdataR4[] r4Field;

            private myhealthbankBdataR5 r5Field;

            private string r6Field;

            private myhealthbankBdataR7[] r7Field;

            private myhealthbankBdataR8[] r8Field;

            private string r9Field;

            private string r10Field;

            private myhealthbankBdataR11 r11Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("b1.1")]
            public string b11
            {
                get
                {
                    return this.b11Field;
                }
                set
                {
                    this.b11Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("b1.2")]
            public uint b12
            {
                get
                {
                    return this.b12Field;
                }
                set
                {
                    this.b12Field = value;
                }
            }

            /// <remarks/>
            public string r0
            {
                get
                {
                    return this.r0Field;
                }
                set
                {
                    this.r0Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1")]
            public myhealthbankBdataR1[] r1
            {
                get
                {
                    return this.r1Field;
                }
                set
                {
                    this.r1Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2")]
            public myhealthbankBdataR2[] r2
            {
                get
                {
                    return this.r2Field;
                }
                set
                {
                    this.r2Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3")]
            public myhealthbankBdataR3[] r3
            {
                get
                {
                    return this.r3Field;
                }
                set
                {
                    this.r3Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4")]
            public myhealthbankBdataR4[] r4
            {
                get
                {
                    return this.r4Field;
                }
                set
                {
                    this.r4Field = value;
                }
            }

            /// <remarks/>
            public myhealthbankBdataR5 r5
            {
                get
                {
                    return this.r5Field;
                }
                set
                {
                    this.r5Field = value;
                }
            }

            /// <remarks/>
            public string r6
            {
                get
                {
                    return this.r6Field;
                }
                set
                {
                    this.r6Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7")]
            public myhealthbankBdataR7[] r7
            {
                get
                {
                    return this.r7Field;
                }
                set
                {
                    this.r7Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8")]
            public myhealthbankBdataR8[] r8
            {
                get
                {
                    return this.r8Field;
                }
                set
                {
                    this.r8Field = value;
                }
            }

            /// <remarks/>
            public string r9
            {
                get
                {
                    return this.r9Field;
                }
                set
                {
                    this.r9Field = value;
                }
            }

            /// <remarks/>
            public string r10
            {
                get
                {
                    return this.r10Field;
                }
                set
                {
                    this.r10Field = value;
                }
            }

            /// <remarks/>
            public myhealthbankBdataR11 r11
            {
                get
                {
                    return this.r11Field;
                }
                set
                {
                    this.r11Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR1
        {

            private byte r11Field;

            private string r12Field;

            private ulong r13Field;

            private string r14Field;

            private string r15Field;

            private string r16Field;

            private string r17Field;

            private string r18Field;

            private string r19Field;

            private string r110Field;

            private string r111Field;

            private ushort r112Field;

            private uint r113Field;

            private myhealthbankBdataR1R1_1[] r1_1Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.1")]
            public byte r11
            {
                get
                {
                    return this.r11Field;
                }
                set
                {
                    this.r11Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.2")]
            public string r12
            {
                get
                {
                    return this.r12Field;
                }
                set
                {
                    this.r12Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.3")]
            public ulong r13
            {
                get
                {
                    return this.r13Field;
                }
                set
                {
                    this.r13Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.4")]
            public string r14
            {
                get
                {
                    return this.r14Field;
                }
                set
                {
                    this.r14Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.5")]
            public string r15
            {
                get
                {
                    return this.r15Field;
                }
                set
                {
                    this.r15Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.6")]
            public string r16
            {
                get
                {
                    return this.r16Field;
                }
                set
                {
                    this.r16Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.7")]
            public string r17
            {
                get
                {
                    return this.r17Field;
                }
                set
                {
                    this.r17Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.8")]
            public string r18
            {
                get
                {
                    return this.r18Field;
                }
                set
                {
                    this.r18Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.9")]
            public string r19
            {
                get
                {
                    return this.r19Field;
                }
                set
                {
                    this.r19Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.10")]
            public string r110
            {
                get
                {
                    return this.r110Field;
                }
                set
                {
                    this.r110Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.11")]
            public string r111
            {
                get
                {
                    return this.r111Field;
                }
                set
                {
                    this.r111Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.12")]
            public ushort r112
            {
                get
                {
                    return this.r112Field;
                }
                set
                {
                    this.r112Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1.13")]
            public uint r113
            {
                get
                {
                    return this.r113Field;
                }
                set
                {
                    this.r113Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1_1")]
            public myhealthbankBdataR1R1_1[] r1_1
            {
                get
                {
                    return this.r1_1Field;
                }
                set
                {
                    this.r1_1Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR1R1_1
        {

            private string r1_11Field;

            private string r1_12Field;

            private decimal r1_13Field;

            private byte r1_14Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1_1.1")]
            public string r1_11
            {
                get
                {
                    return this.r1_11Field;
                }
                set
                {
                    this.r1_11Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1_1.2")]
            public string r1_12
            {
                get
                {
                    return this.r1_12Field;
                }
                set
                {
                    this.r1_12Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1_1.3")]
            public decimal r1_13
            {
                get
                {
                    return this.r1_13Field;
                }
                set
                {
                    this.r1_13Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r1_1.4")]
            public byte r1_14
            {
                get
                {
                    return this.r1_14Field;
                }
                set
                {
                    this.r1_14Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR2
        {

            private byte r21Field;

            private string r22Field;

            private uint r23Field;

            private string r24Field;

            private string r25Field;

            private string r26Field;

            private object r27Field;

            private object r28Field;

            private string r29Field;

            private string r210Field;

            private string r211Field;

            private object r212Field;

            private object r213Field;

            private byte r214Field;

            private uint r215Field;

            private myhealthbankBdataR2R2_1 r2_1Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.1")]
            public byte r21
            {
                get
                {
                    return this.r21Field;
                }
                set
                {
                    this.r21Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.2")]
            public string r22
            {
                get
                {
                    return this.r22Field;
                }
                set
                {
                    this.r22Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.3")]
            public uint r23
            {
                get
                {
                    return this.r23Field;
                }
                set
                {
                    this.r23Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.4")]
            public string r24
            {
                get
                {
                    return this.r24Field;
                }
                set
                {
                    this.r24Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.5")]
            public string r25
            {
                get
                {
                    return this.r25Field;
                }
                set
                {
                    this.r25Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.6")]
            public string r26
            {
                get
                {
                    return this.r26Field;
                }
                set
                {
                    this.r26Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.7")]
            public object r27
            {
                get
                {
                    return this.r27Field;
                }
                set
                {
                    this.r27Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.8")]
            public object r28
            {
                get
                {
                    return this.r28Field;
                }
                set
                {
                    this.r28Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.9")]
            public string r29
            {
                get
                {
                    return this.r29Field;
                }
                set
                {
                    this.r29Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.10")]
            public string r210
            {
                get
                {
                    return this.r210Field;
                }
                set
                {
                    this.r210Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.11")]
            public string r211
            {
                get
                {
                    return this.r211Field;
                }
                set
                {
                    this.r211Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.12")]
            public object r212
            {
                get
                {
                    return this.r212Field;
                }
                set
                {
                    this.r212Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.13")]
            public object r213
            {
                get
                {
                    return this.r213Field;
                }
                set
                {
                    this.r213Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.14")]
            public byte r214
            {
                get
                {
                    return this.r214Field;
                }
                set
                {
                    this.r214Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2.15")]
            public uint r215
            {
                get
                {
                    return this.r215Field;
                }
                set
                {
                    this.r215Field = value;
                }
            }

            /// <remarks/>
            public myhealthbankBdataR2R2_1 r2_1
            {
                get
                {
                    return this.r2_1Field;
                }
                set
                {
                    this.r2_1Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR2R2_1
        {

            private uint r2_11Field;

            private uint r2_12Field;

            private uint r2_13Field;

            private string r2_14Field;

            private string r2_15Field;

            private decimal r2_16Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2_1.1")]
            public uint r2_11
            {
                get
                {
                    return this.r2_11Field;
                }
                set
                {
                    this.r2_11Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2_1.2")]
            public uint r2_12
            {
                get
                {
                    return this.r2_12Field;
                }
                set
                {
                    this.r2_12Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2_1.3")]
            public uint r2_13
            {
                get
                {
                    return this.r2_13Field;
                }
                set
                {
                    this.r2_13Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2_1.4")]
            public string r2_14
            {
                get
                {
                    return this.r2_14Field;
                }
                set
                {
                    this.r2_14Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2_1.5")]
            public string r2_15
            {
                get
                {
                    return this.r2_15Field;
                }
                set
                {
                    this.r2_15Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r2_1.6")]
            public decimal r2_16
            {
                get
                {
                    return this.r2_16Field;
                }
                set
                {
                    this.r2_16Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR3
        {

            private byte r31Field;

            private string r32Field;

            private uint r33Field;

            private string r34Field;

            private uint r35Field;

            private byte r36Field;

            private string r37Field;

            private string r38Field;

            private string r39Field;

            private string r310Field;

            private byte r311Field;

            private ushort r312Field;

            private myhealthbankBdataR3R3_1[] r3_1Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.1")]
            public byte r31
            {
                get
                {
                    return this.r31Field;
                }
                set
                {
                    this.r31Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.2")]
            public string r32
            {
                get
                {
                    return this.r32Field;
                }
                set
                {
                    this.r32Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.3")]
            public uint r33
            {
                get
                {
                    return this.r33Field;
                }
                set
                {
                    this.r33Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.4")]
            public string r34
            {
                get
                {
                    return this.r34Field;
                }
                set
                {
                    this.r34Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.5")]
            public uint r35
            {
                get
                {
                    return this.r35Field;
                }
                set
                {
                    this.r35Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.6")]
            public byte r36
            {
                get
                {
                    return this.r36Field;
                }
                set
                {
                    this.r36Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.7")]
            public string r37
            {
                get
                {
                    return this.r37Field;
                }
                set
                {
                    this.r37Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.8")]
            public string r38
            {
                get
                {
                    return this.r38Field;
                }
                set
                {
                    this.r38Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.9")]
            public string r39
            {
                get
                {
                    return this.r39Field;
                }
                set
                {
                    this.r39Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.10")]
            public string r310
            {
                get
                {
                    return this.r310Field;
                }
                set
                {
                    this.r310Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.11")]
            public byte r311
            {
                get
                {
                    return this.r311Field;
                }
                set
                {
                    this.r311Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3.12")]
            public ushort r312
            {
                get
                {
                    return this.r312Field;
                }
                set
                {
                    this.r312Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1")]
            public myhealthbankBdataR3R3_1[] r3_1
            {
                get
                {
                    return this.r3_1Field;
                }
                set
                {
                    this.r3_1Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR3R3_1
        {

            private string r3_11Field;

            private string r3_12Field;

            private decimal r3_13Field;

            private string r3_14Field;

            private string r3_15Field;

            private byte r3_16Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1.1")]
            public string r3_11
            {
                get
                {
                    return this.r3_11Field;
                }
                set
                {
                    this.r3_11Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1.2")]
            public string r3_12
            {
                get
                {
                    return this.r3_12Field;
                }
                set
                {
                    this.r3_12Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1.3")]
            public decimal r3_13
            {
                get
                {
                    return this.r3_13Field;
                }
                set
                {
                    this.r3_13Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1.4")]
            public string r3_14
            {
                get
                {
                    return this.r3_14Field;
                }
                set
                {
                    this.r3_14Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1.5")]
            public string r3_15
            {
                get
                {
                    return this.r3_15Field;
                }
                set
                {
                    this.r3_15Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r3_1.6")]
            public byte r3_16
            {
                get
                {
                    return this.r3_16Field;
                }
                set
                {
                    this.r3_16Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR4
        {

            private uint r41Field;

            private string r42Field;

            private string r43Field;

            private uint r44Field;

            private string r45Field;

            private byte r46Field;

            private uint r47Field;

            private string r48Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.1")]
            public uint r41
            {
                get
                {
                    return this.r41Field;
                }
                set
                {
                    this.r41Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.2")]
            public string r42
            {
                get
                {
                    return this.r42Field;
                }
                set
                {
                    this.r42Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.3")]
            public string r43
            {
                get
                {
                    return this.r43Field;
                }
                set
                {
                    this.r43Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.4")]
            public uint r44
            {
                get
                {
                    return this.r44Field;
                }
                set
                {
                    this.r44Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.5")]
            public string r45
            {
                get
                {
                    return this.r45Field;
                }
                set
                {
                    this.r45Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.6")]
            public byte r46
            {
                get
                {
                    return this.r46Field;
                }
                set
                {
                    this.r46Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.7")]
            public uint r47
            {
                get
                {
                    return this.r47Field;
                }
                set
                {
                    this.r47Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r4.8")]
            public string r48
            {
                get
                {
                    return this.r48Field;
                }
                set
                {
                    this.r48Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR5
        {

            private uint r51Field;

            private string r52Field;

            private string r53Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r5.1")]
            public uint r51
            {
                get
                {
                    return this.r51Field;
                }
                set
                {
                    this.r51Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r5.2")]
            public string r52
            {
                get
                {
                    return this.r52Field;
                }
                set
                {
                    this.r52Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r5.3")]
            public string r53
            {
                get
                {
                    return this.r53Field;
                }
                set
                {
                    this.r53Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR7
        {

            private byte r71Field;

            private string r72Field;

            private uint r73Field;

            private string r74Field;

            private uint r75Field;

            private uint r76Field;

            private ulong r77Field;

            private string r78Field;

            private string r79Field;

            private string r710Field;

            private string r711Field;

            private string r712Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.1")]
            public byte r71
            {
                get
                {
                    return this.r71Field;
                }
                set
                {
                    this.r71Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.2")]
            public string r72
            {
                get
                {
                    return this.r72Field;
                }
                set
                {
                    this.r72Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.3")]
            public uint r73
            {
                get
                {
                    return this.r73Field;
                }
                set
                {
                    this.r73Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.4")]
            public string r74
            {
                get
                {
                    return this.r74Field;
                }
                set
                {
                    this.r74Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.5")]
            public uint r75
            {
                get
                {
                    return this.r75Field;
                }
                set
                {
                    this.r75Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.6")]
            public uint r76
            {
                get
                {
                    return this.r76Field;
                }
                set
                {
                    this.r76Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.7")]
            public ulong r77
            {
                get
                {
                    return this.r77Field;
                }
                set
                {
                    this.r77Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.8")]
            public string r78
            {
                get
                {
                    return this.r78Field;
                }
                set
                {
                    this.r78Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.9")]
            public string r79
            {
                get
                {
                    return this.r79Field;
                }
                set
                {
                    this.r79Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.10")]
            public string r710
            {
                get
                {
                    return this.r710Field;
                }
                set
                {
                    this.r710Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.11")]
            public string r711
            {
                get
                {
                    return this.r711Field;
                }
                set
                {
                    this.r711Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r7.12")]
            public string r712
            {
                get
                {
                    return this.r712Field;
                }
                set
                {
                    this.r712Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR8
        {

            private byte r81Field;

            private string r82Field;

            private uint r83Field;

            private string r84Field;

            private uint r85Field;

            private uint r86Field;

            private ulong r87Field;

            private string r88Field;

            private string r89Field;

            private string r810Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.1")]
            public byte r81
            {
                get
                {
                    return this.r81Field;
                }
                set
                {
                    this.r81Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.2")]
            public string r82
            {
                get
                {
                    return this.r82Field;
                }
                set
                {
                    this.r82Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.3")]
            public uint r83
            {
                get
                {
                    return this.r83Field;
                }
                set
                {
                    this.r83Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.4")]
            public string r84
            {
                get
                {
                    return this.r84Field;
                }
                set
                {
                    this.r84Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.5")]
            public uint r85
            {
                get
                {
                    return this.r85Field;
                }
                set
                {
                    this.r85Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.6")]
            public uint r86
            {
                get
                {
                    return this.r86Field;
                }
                set
                {
                    this.r86Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.7")]
            public ulong r87
            {
                get
                {
                    return this.r87Field;
                }
                set
                {
                    this.r87Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.8")]
            public string r88
            {
                get
                {
                    return this.r88Field;
                }
                set
                {
                    this.r88Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.9")]
            public string r89
            {
                get
                {
                    return this.r89Field;
                }
                set
                {
                    this.r89Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r8.10")]
            public string r810
            {
                get
                {
                    return this.r810Field;
                }
                set
                {
                    this.r810Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR11
        {

            private string r111Field;

            private string r112Field;

            private myhealthbankBdataR11R11_1 r11_1Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r11.1")]
            public string r111
            {
                get
                {
                    return this.r111Field;
                }
                set
                {
                    this.r111Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r11.2")]
            public string r112
            {
                get
                {
                    return this.r112Field;
                }
                set
                {
                    this.r112Field = value;
                }
            }

            /// <remarks/>
            public myhealthbankBdataR11R11_1 r11_1
            {
                get
                {
                    return this.r11_1Field;
                }
                set
                {
                    this.r11_1Field = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class myhealthbankBdataR11R11_1
        {

            private uint r11_11Field;

            private string r11_12Field;

            private string r11_13Field;

            private string r11_14Field;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r11_1.1")]
            public uint r11_11
            {
                get
                {
                    return this.r11_11Field;
                }
                set
                {
                    this.r11_11Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r11_1.2")]
            public string r11_12
            {
                get
                {
                    return this.r11_12Field;
                }
                set
                {
                    this.r11_12Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r11_1.3")]
            public string r11_13
            {
                get
                {
                    return this.r11_13Field;
                }
                set
                {
                    this.r11_13Field = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("r11_1.4")]
            public string r11_14
            {
                get
                {
                    return this.r11_14Field;
                }
                set
                {
                    this.r11_14Field = value;
                }
            }
        }



    }
}