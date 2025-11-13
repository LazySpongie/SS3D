using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SS3D.Systems.Roles
{
    /// <summary>
    /// All the relevant data for a department
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "Department Data", menuName = "Roles/DepartmentData")]
    public class DepartmentData : ScriptableObject
    {
        [SerializeField] private string _departmentName;
        
        [SerializeField] private List<RoleData> _chainOfCommand;

        public string Name => _departmentName;

        public ReadOnlyCollection<RoleData> ChainOfCommand => _chainOfCommand.AsReadOnly();

    }
}
