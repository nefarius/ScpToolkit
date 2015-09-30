using System.Collections.Generic;

namespace ScpControl.Driver.PNPUtilLib
{
    public interface IDriverStore
    {
        List<DriverStoreEntry> EnumeratePackages();
        bool DeletePackage(DriverStoreEntry dse, bool forceDelete);
        bool AddPackage(string infFullPath, bool install);
    }
}
