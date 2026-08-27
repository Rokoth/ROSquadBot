namespace ROTGBot.Contract.Model
{
    public enum RoleEnum
    {
        user    =  0,
        squaddie =  1,
        district_commander = 2,
        city_commander  = 3,
        administrator =  4,
    }
    
    public enum CommandType
    {
        AddSquaddie,
        AddCommander,
        AddDistrictCommander,
        AddAdministrator,
        AddSquaddieResponse,
        AddCommanderResponse,
        AddDistrictCommanderResponse,
        AddAdministratorResponse,      
        ViewDemands,
        ViewDemandsResponse,        
        ViewUserRights,
        ViewUserRightsResponse,        
        AddUserRights,
        AddUserRightsResponse,
        DeleteUserRights,
        DeleteUserRightsResponse,        
        DeclineCurrentTask,
    }
}
