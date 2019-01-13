using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoplarCloud.Interfaces
{
    public interface IPacket
    {
        /// <summary>
        /// 打包数据
        /// </summary>
        /// <returns></returns>
        byte[] Encoder();
    }
}
