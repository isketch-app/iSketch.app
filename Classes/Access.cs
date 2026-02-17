using System.Collections.Generic;

namespace iSketch.app.Classes;

public static class Access
{
    public enum Permission
    {
        Administrator, //Main administrator page.
        Words //Words administrator page.
    }
    public class Permissions : List<Permission> { }
}