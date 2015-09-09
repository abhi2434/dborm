using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBOrm.TableAccessors
{
    public interface ITableRow
    {
        void Save();
        void Remove();
        void Dispose();
        void SetDirty(bool val);
    }
}
