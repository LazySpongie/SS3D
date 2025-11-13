using System;
using System.Collections.Generic;

namespace SS3D.Systems.Roles
{
    /// <summary>
    /// Represents a department and which roles are available for it
    /// </summary>
    [Serializable]
    public class DepartmentsData
    {
        public DepartmentData Department;
        public List<RolesData> Roles;
    }
}

