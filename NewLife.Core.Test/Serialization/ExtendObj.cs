﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using NewLife.Serialization;

namespace NewLife.Core.Test.Serialization
{
    class ExtendObj : SimpleObj
    {
        #region 属性
        private Byte[] _Bts;
        /// <summary>属性说明</summary>
        public Byte[] Bts { get { return _Bts; } set { _Bts = value; } }

        private Char[] _Cs;
        /// <summary>属性说明</summary>
        public Char[] Cs { get { return _Cs; } set { _Cs = value; } }

        private Guid _G;
        /// <summary>属性说明</summary>
        public Guid G { get { return _G; } set { _G = value; } }

        private IPAddress _Address;
        /// <summary>属性说明</summary>
        public IPAddress Address { get { return _Address; } set { _Address = value; } }

        private IPEndPoint _EndPoint;
        /// <summary>属性说明</summary>
        public IPEndPoint EndPoint { get { return _EndPoint; } set { _EndPoint = value; } }

        private Type _T;
        /// <summary>属性说明</summary>
        public Type T { get { return _T; } set { _T = value; } }
        #endregion

        #region 方法
        public new static ExtendObj Create()
        {
            var obj = new ExtendObj();
            obj.OnInit();

            return obj;
        }

        protected override void OnInit()
        {
            base.OnInit();

            var r = Rnd;
            //if (r.Next(10) == 0) return;

            // 减去1，可能出现-1，这样子就做到可能有0字节数组，也可能为null
            var n = r.Next(256) - 1;
            if (n >= 0)
            {
                Bts = new Byte[n];
                r.NextBytes(Bts);
            }

            n = r.Next(10);
            if (n > 0)
            {
                if (Str != null)
                    Cs = Str.ToArray();
                else
                    Cs = new Char[0];
            }

            if (r.Next(10) > 0) G = Guid.NewGuid();

            if (r.Next(10) > 0)
            {
                var buf = new Byte[r.Next(2) == 0 ? 4 : 16];
                r.NextBytes(buf);
                Address = new IPAddress(buf);
                EndPoint = new IPEndPoint(Address, r.Next(65536));
            }

            if (r.Next(10) > 0)
            {
                var ts = typeof(IReaderWriter).Assembly.GetTypes();
                do
                {
                    T = ts[r.Next(ts.Length)];
                } while (T.IsArray || T.IsNested || T.IsGenericType && !T.IsGenericTypeDefinition);
            }
        }

        public override void Write(BinaryWriter writer, BinarySettings set)
        {
            base.Write(writer, set);

            var encodeSize = set.EncodeInt || ((Int32)set.SizeFormat % 2 == 0);
            if (Bts == null)
                writer.WriteInt((Int32)0, encodeSize);
            else
            {
                writer.WriteInt(Bts.Length, encodeSize);
                writer.Write(Bts);
            }

            if (Cs == null)
                writer.WriteInt((Int32)0, encodeSize);
            else
            {
                var buf = set.Encoding.GetBytes(Cs);
                writer.WriteInt(buf.Length, encodeSize);
                writer.Write(buf);
            }

            writer.Write(G.ToByteArray());

            if (Address == null)
                writer.WriteInt((Int32)0, encodeSize);
            else
            {
                var buf = Address.GetAddressBytes();
                writer.WriteInt(buf.Length, encodeSize);
                writer.Write(buf);
            }

            if (EndPoint == null)
                writer.WriteInt((Int32)0, encodeSize);
            else
            {
                var buf = EndPoint.Address.GetAddressBytes();
                writer.WriteInt(buf.Length, encodeSize);
                writer.Write(buf);

                // 纯编码整数，与大小无关
                writer.WriteInt(EndPoint.Port, set.EncodeInt);
            }

            if (T == null)
                writer.WriteInt((Int32)0, encodeSize);
            else
            {
                //writer.Write(T.FullName);
                if (set.SplitComplexType) writer.Write((Byte)BinarySettings.TypeKinds.Normal);
                if (set.UseTypeFullName)
                    writer.Write(T.AssemblyQualifiedName);
                else
                    writer.Write(T.FullName);
            }
        }
        #endregion
    }
}