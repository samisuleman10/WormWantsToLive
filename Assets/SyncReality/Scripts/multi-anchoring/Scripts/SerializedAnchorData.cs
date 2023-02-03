//using System;
//using System.Collections.Generic;
//using System.Linq;
////using Syncreality.Meshify;
//using UnityEngine;

//namespace Syncreality
//{
//    public class SerializedAnchorData : IEquatable<SerializedAnchorData>
//    {
//        /// <summary>
//        /// String representation of the unique ID of the anchor
//        /// </summary>
//        public string spaceUuid;

//        /// <summary>
//        /// Serialized Guardians
//        /// </summary>
//        public MeshifiedScan guardians;

//        //public List<LocalPositionByAnchor> localPositionsByAnchor;

//        /// <summary>
//        /// Serialized OculusCameraRig Position
//        /// </summary>
//        public ScannedObject xrRigTransform;



//        public bool Equals(SerializedAnchorData other)
//        {
//            if (other == null) return false;
//            if (spaceUuid != other.spaceUuid) return false;
//            if (guardians.scannedObjects.Count != other.guardians.scannedObjects.Count) return false;
//            foreach (var guardian in guardians.scannedObjects)
//                if (guardians.scannedObjects.Any(g => guardian.Equals(g) == false))
//                    return false;
//            if (xrRigTransform.Equals(other.xrRigTransform) == false) return false;

//            return true;
//        }

//        public SerializedAnchorData(string spaceUuid, MeshifiedScan guardians, ScannedObject xrRigTransform)
//        {//List<LocalPositionByAnchor> localPositionsByAnchor
//            this.spaceUuid = spaceUuid;
//            this.guardians = guardians;
//            this.xrRigTransform = xrRigTransform;
//            //this.localPositionsByAnchor = localPositionsByAnchor;
//        }
//    }

//    public class LocalPositionByAnchor
//    {
//        public string uuid;
//        public Vector3 localPosition;
//    }
//}
