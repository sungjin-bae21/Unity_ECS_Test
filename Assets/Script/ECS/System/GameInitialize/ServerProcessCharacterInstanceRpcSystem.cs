using UnityEngine;
using Unity.NetCode;
using Unity.Entities;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerProcessCharacterInstanceRpcSystem : ComponentSystem
{

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<GhostPrefabCollectionComponent>();
    }


    protected override void OnUpdate()
    {
        Entities.ForEach(
            (Entity ent_, ref CharacterInstanceRpc instance_info_, ref ReceiveRpcCommandRequestComponent req_) =>
            {
                Entity character_prefab =
                    GhostPrefabLoader.GetCharacterPrefab(EntityManager,
                                                         GetSingletonEntity<GhostPrefabCollectionComponent>(),
                                                         instance_info_.character_class);
                Entity character = EntityManager.Instantiate(character_prefab);
#if UNITY_EDITOR
                EntityManager.SetName(character, "PlayerCharacter");
#endif
                int network_id = EntityManager.GetComponentData<NetworkIdComponent>(req_.SourceConnection).Value;
                EntityManager.SetComponentData(character,
                    new GhostOwnerComponent { NetworkId = network_id });

                BindCharacterToCommandTargetComponent(network_id, character);

                Entity skill_collections = GetOrCreateSkillCollectionBuffer(network_id);

                var buffer = EntityManager.GetBuffer<InGameSkillPrefabBuffer>(skill_collections);
                var skill_prefab =
                    GhostPrefabLoader.GetSkillPerfab(EntityManager,
                                                     GetSingletonEntity<GhostPrefabCollectionComponent>(),
                                                     instance_info_.skill1);

                buffer.Add(new InGameSkillPrefabBuffer { Value = skill_prefab, skill_name = instance_info_.skill1 });

                Entity ui_data_prefab =
                    GhostPrefabLoader.GetUIDataPrefab(EntityManager,
                                                      GetSingletonEntity<GhostPrefabCollectionComponent>());

                Entity ui_data = EntityManager.Instantiate(ui_data_prefab);
                EntityManager.SetComponentData(ui_data, new PlayerHPUIComponent { hp = 100 });
                EntityManager.SetComponentData(ui_data, new UIEntityToNetworkIDComponent { network_id = network_id });
                PostUpdateCommands.DestroyEntity(ent_);
            });
    }


    public Entity GetOrCreateSkillCollectionBuffer(int network_id_)
    {
        Entity ent = Entity.Null;
        Entities.ForEach(
            (Entity ent_, ref SkillToNetworkIDComponent info_) => {
                if (info_.network_id == network_id_)
                {
                    ent = ent_;
                    return;
                }
            });

        if (ent != Entity.Null)
        {
            return ent;
        }

        ent = EntityManager.CreateEntity();
        EntityManager.AddComponent<SkillToNetworkIDComponent>(ent);
        EntityManager.SetComponentData<SkillToNetworkIDComponent>(ent, new SkillToNetworkIDComponent { network_id = network_id_ });
        EntityManager.AddBuffer<InGameSkillPrefabBuffer>(ent);
        return ent;
    }


    public void BindCharacterToCommandTargetComponent(int client_id_, Entity instance_character_)
    {
        Entities.ForEach(
            (ref CommandTargetComponent command_target, ref NetworkIdComponent network_id) =>
            {
                if (network_id.Value != client_id_)
                {
                    return;
                }

                command_target.targetEntity = instance_character_;
            });
    }
       

}