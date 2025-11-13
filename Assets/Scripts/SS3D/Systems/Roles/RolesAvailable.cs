using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Roles
{
    /// <summary>
    /// List of all departments and roles available in the gamemode
    /// </summary>
    [CreateAssetMenu(fileName = "Role Data", menuName = "Roles/Roles")]
    public class RolesAvailable : ScriptableObject
    {
        /// <summary>
        /// The default job (assistant)
        /// </summary>
        public RoleData OverFlowRole;

        /// <summary>
        /// Jobs that the game will assign before anything else
        /// </summary>
        public List<RoleData> PriorityRoles;

        /// <summary>
        /// Departments that each contain a list of jobs with slots
        /// </summary>
        public List<DepartmentsData> Departments;
    }
}
