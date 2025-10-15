using Projects;
using YamlDotNet.Core;

var builder = DistributedApplication.CreateBuilder(args);
// Add the chat application
builder.AddProject<Projects.Semantic_WebApp>("semantic-webapp");

builder.Build().Run();
