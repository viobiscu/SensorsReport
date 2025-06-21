using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrionldClient
{
    internal static class KrakendMapping
    {
        //Entities
        public static readonly string OrionGETEntities              = @"/orionld/ngsi-ld/v1/entities";
        public static readonly string OrionPATCHEntities            = @"/orionld/ngsi-ld/v1/entities/{entityId}";
        public static readonly string OrionGETEntity                = @"/orionld/ngsi-ld/v1/entities/{entityId}";
        public static readonly string OrionDELETEEntities           = @"/orionld/ngsi-ld/v1/entities/{entityId}";
        public static readonly string OrionPOSTEntities             = @"/entities";
        public static readonly string OrionPATCHEntity              = @"/entities/{entityId}";
        public static readonly string OrionPUTEntity                = @"/entities/{entityId}";
        public static readonly string OrionDELETEEntities2          = @"/entities";
        public static readonly string OrionDELETETEntity            = @"/entities/{entityId}";
        public static readonly string OrionGETEntity2               = @"/entities/{entityId}";
        public static readonly string OrionPOSTQueryEntities        = @"/entityOperations/query";
        public static readonly string OrionPOSTEntityOperation      = @"/entityOperations/create";
        public static readonly string OrionPOSTEntityOperationMerge = @"/entityOperations/merge";
        public static readonly string OrionPOSTEntityDelete         = @"/entityOperations/delete";
        public static readonly string OrionPOSTEntityUpdate         = @"/entityOperations/update";

        //Context
        public static readonly string OrionGetContexts = @"/jsonldContexts";
        public static readonly string OrionPOSTContextsEntity = @"/jsonldContexts";
        public static readonly string OrionDELETEContexts = @"/jsonldContexts/{contextId}";
        public static readonly string OrionGETContexts = @"/jsonldContexts/{contextId}";

        //Attribute
        public static readonly string OrionDELETEAtribute2 = @"/entities/{entityId}/attrs/{attrId}";
        public static readonly string OrionPATCHEntities2 = @"/entities/{entityId}/attrs/{attrId}";
        public static readonly string OrionPutEntityAtribute = @"/entities/{entityId}/attrs/{attrId}";
        public static readonly string OrionPOSTAttribute = @"/entities/{entityId}/attrs";
        public static readonly string OrionGETAttributes = @"/attributes";
        public static readonly string OrionDELETEAtribute = @"/orionld/ngsi-ld/v1/entities/{entityId}/attrs/{attrId}";
        public static readonly string OrionGETAttribute = @"/attributes/{attrId}";

        //Subscription
        public static readonly string OrionGetSubscription      = @"/csourceSubscriptions";
        public static readonly string OrionPOSTSubscription2    = @"/csourceSubscriptions";
        public static readonly string OrionUPSERTSubscription   = @"/entityOperations/upsert";
        public static readonly string OrionDELETESubscription   = @"/csourceSubscriptions/{subscriptionId}";
        public static readonly string OrionGETSubscription      = @"/csourceSubscriptions/{subscriptionId}";
        public static readonly string OrionPATCHSubscription    = @"/csourceSubscriptions/{subscriptionId}";
        public static readonly string OrionPOSTSubscription     = @"/orionld/ngsi-ld/v1/subscriptions/{subscriptionId}";
        public static readonly string OrionPOSTSubscriptions    = @"/subscriptions";
        public static readonly string OrionGETSubscriptions     = @"/subscriptions";
        public static readonly string OrionDELETESubscription2  = @"/subscriptions/{subscriptionId}";
        public static readonly string OrionGETSubscription2     = @"/subscriptions/{subscriptionId}";
        public static readonly string OrionPATCHSubscription2   = @"/subscriptions/{subscriptionId}";

        //Types
        public static readonly string OrionGetType = @"/types/{type}";
        public static readonly string OrionGETTypes = @"/types";

        //Registrations
        public static readonly string OrionGETRegistrations = @"/csourceRegistrations";
        public static readonly string OrionPOSTRegistrations = @"/csourceRegistrations";
        public static readonly string OrionDELETERegistration = @"/csourceRegistrations/{registrationId}";
        public static readonly string OrionGETRegistration = @"/csourceRegistrations/{registrationId}";
        public static readonly string OrionPATCHRegistration = @"/csourceRegistrations/{registrationId}";

        //Notify
        public static readonly string OrionPOSTNotify = @"/orionld/ex/v1/notify";
        public static readonly string OrionPOSTNotify2 = @"/orionld/ex/v1/notify/notifications/{subscriptionId}";











    }
}
