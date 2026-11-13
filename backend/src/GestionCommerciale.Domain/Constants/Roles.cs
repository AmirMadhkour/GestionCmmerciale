namespace GestionCommerciale.Domain.Constants;

public static class Roles
{
    public const string ADMIN = "ADMIN";
    public const string USER = "USER";
    public const string COMMERCIAL = "COMMERCIAL";
    public const string ALL = "ADMIN,USER,COMMERCIAL";
    public const string ADMINUSER="ADMIN,USER";  //An administrator should also be able to perform normal user operations.
}                                               // so its a boath role