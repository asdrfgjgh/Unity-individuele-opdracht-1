using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PostWereldLoadResponseDto
{
    public string id;
    public string name;
    public string ownerUserId;
    public int maxLength;
    public int maxHeight;
}

[Serializable]
public class GetWorldsResponseDto
{
    public List<PostWereldLoadResponseDto> worlds;
}