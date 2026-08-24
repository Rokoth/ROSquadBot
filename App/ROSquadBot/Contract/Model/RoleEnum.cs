namespace ROTGBot.Contract.Model
{
    public enum RoleEnum
    {
        administrator,
        district_commander,
        city_commander,
        squaddie,
        user
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
