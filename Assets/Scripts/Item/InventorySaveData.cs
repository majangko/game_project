using System;
using System.Collections.Generic;

[Serializable] public class InventorySaveData { public List<ItemStack> Items = new(); }
[Serializable] public struct ItemStack { public string ItemId; public int Count; public ItemStack(string id,int c){ItemId=id;Count=c;} }
