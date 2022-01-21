using System;
using System.Collections;
using System.Collections.Generic;
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

[System.Serializable]
public class ExerciseData
{
    public Actor actor                  /*{ get; set; }*/ ;
    public Verb verb                    /*{ get; set; }*/ ;
    public Object @object               /*{ get; set; }*/ ;
    public Result result                /*{ get; set; }*/ ;
    public Context context              /*{ get; set; }*/ ;
    public string stored              /*{ get; set; }*/ ;
    public Authority authority          /*{ get; set; }*/ ;
    public string version               /*{ get; set; }*/ ;
    public string id                    /*{ get; set; }*/ ;
    public string timestamp           /*{ get; set; }*/ ;
    public List<Attachment> attachments /*{ get; set; }*/ ;
}

[System.Serializable]
public class Actor
{
    public string name       /*{ get; set; }*/  ;
    public string mbox       /*{ get; set; }*/  ;
    public string objectType /*{ get; set; }*/  ;
}

[System.Serializable]
public class Display
{
    //[JsonProperty("en-GB")]
    public string EnGB        /*{ get; set; }*/  ;

    // [JsonProperty("en-US")]
    public string EnUS        /*{ get; set; }*/  ; 
}

[System.Serializable]
public class Verb
{
    public string id       /*{ get; set; }*/  ;
    public Display display /*{ get; set; }*/  ;
}

[System.Serializable]
public class Name
{
    // [JsonProperty("en-GB")]
    public string EnGB         /*{ get; set; }*/ ;

    // [JsonProperty("en-US")]
    public string EnUS        /* { get; set; }*/ ;
}

[System.Serializable]
public class Description
{
    // [JsonProperty("en-GB")]
    public string EnGB       /* { get; set; }*/ ;

    // [JsonProperty("en-US")]
    public string EnUS       /* { get; set; }*/ ;
}

[System.Serializable]
public class Definition
{
    public Name name               /*{ get; set; }*/  ;
    public Description description /*{ get; set; }*/  ;
    public string type             /*{ get; set; }*/  ;
}

[System.Serializable]
public class Object
{
    public string id             /*{ get; set; }*/ ;
    public Definition definition /*{ get; set; }*/ ;
    public string objectType     /*{ get; set; }*/ ;
}

[System.Serializable]
public class Result
{
    public bool success    /*{ get; set; }*/ ;
    public bool completion /*{ get; set; }*/ ;
    public string duration /*{ get; set; }*/ ;
    public string response;
}

[System.Serializable]
public class Parent
{
    public string id              /* { get; set; }*/  ;
    public string objectType      /* { get; set; }*/  ;
    public Definition definition  /* { get; set; }*/  ;
}

[System.Serializable]
public class ContextActivities
{
    public List<Parent> parent /*{ get; set; }*/  ;
}

[System.Serializable]
public class Context
{
    public string registration                 /*{ get; set; }*/  ;
    public ContextActivities contextActivities /*{ get; set; }*/  ;
    public string language                     /*{ get; set; }*/  ;
}

[System.Serializable]
public class Account
{
    public string homePage /*{ get; set; }*/   ;
    public string name     /*{ get; set; }*/   ;
}

[System.Serializable]
public class Authority
{
    public Account account   /*{ get; set; }*/  ;
    public string objectType /*{ get; set; }*/  ;
}

[System.Serializable]
public class Attachment
{
    public string contentType      /*{ get; set; }*/     ;
    public string usageType        /*{ get; set; }*/     ;
    public Display display         /*{ get; set; }*/     ;
    public Description description /*{ get; set; }*/     ;
    public int length              /*{ get; set; }*/     ;
    public string sha2             /*{ get; set; }*/     ;
}