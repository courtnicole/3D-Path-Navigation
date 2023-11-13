namespace ExperimentControl
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Text.Json;
    using UnityEngine;

    public static class FileParser
    {
        public static async Task<int> GetNewUserIdAsync(string file)
        {
            int id = await GetNewIdAsync(file);
            return id;
        }
        
        public static async Task<int> GetMostRecentUserIdAsync(string file)
        {
            int id = await GetMostRecentIdAsync(file);
            return id;
        }
        
        private static async Task<int> GetNewIdAsync(string filePath)
        {
            int userId = 99;
            try
            {
                // Read the JSON file asynchronously
                string json;

                using (StreamReader reader = new(filePath))
                {
                    json = await reader.ReadToEndAsync();
                }

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("available_user_ids", out JsonElement availableUserIds))
                    {
                        if (availableUserIds.GetArrayLength() == 0)
                        {
                            Debug.LogError("No available user IDs.");
                            return userId;
                        }

                        int firstAvailableUserId = availableUserIds[0].GetInt32();
                        
                        JsonWriterOptions writerOptions = new() { Indented = true, };
                        using (MemoryStream stream = new())
                        await using (Utf8JsonWriter writer = new(stream, writerOptions))
                        {
                            writer.WriteStartObject();

                            // Update "most_recent_user_id"
                            writer.WriteNumber("most_recent_user_id", firstAvailableUserId);
                            
                            // Remove the first available user ID
                            writer.WriteStartArray("available_user_ids");
                            for (int i = 1; i < availableUserIds.GetArrayLength(); i++)
                            {
                                writer.WriteNumberValue(availableUserIds[i].GetInt32());
                            }
                            writer.WriteEndArray();
                            
                            if (root.TryGetProperty("used_user_ids", out JsonElement usedUserIds))
                            {
                                writer.WriteStartArray("used_user_ids");
                                writer.WriteNumberValue(firstAvailableUserId);

                                foreach (JsonElement id in usedUserIds.EnumerateArray())
                                {
                                    writer.WriteNumberValue(id.GetInt32());
                                }
                                writer.WriteEndArray();
                            }

                            writer.WriteEndObject();

                            await writer.FlushAsync();
                            byte[] jsonBytes = stream.ToArray();
                            await File.WriteAllBytesAsync(filePath, jsonBytes);
                        }
                        
                        userId = firstAvailableUserId;
                        Debug.Log("Reserved User ID: " + firstAvailableUserId);
                    }
                    else
                    {
                        Debug.LogError("The JSON file doesn't contain the expected structure.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred: " + ex.Message);
            }

            return userId;
        }
        
        private static async Task<int> GetMostRecentIdAsync(string filePath)
        {
            int userId = 99;
            try
            {
                // Read the JSON file asynchronously
                string json;

                using (StreamReader reader = new(filePath))
                {
                    json = await reader.ReadToEndAsync();
                }

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("most_recent_user_id", out JsonElement recentUserId))
                    {
                        int mostRecentId = recentUserId.GetInt32();
                        userId = mostRecentId;
                        Debug.Log("Reserved User ID: " + mostRecentId);
                    }
                    else
                    {
                        Debug.LogError("The JSON file doesn't contain the expected structure.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("An error occurred: " + ex.Message);
            }

            return userId;
        }
        
    }
}