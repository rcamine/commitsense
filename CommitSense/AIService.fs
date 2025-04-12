namespace CommitSense

open OpenAI_API
open OpenAI_API.Chat

module AIService =
    let private systemPrompt =
        """
        You are a helpful assistant that generates commit messages based on git diffs.
        Follow these rules:
        1. Keep messages concise and clear
        2. Use imperative mood (e.g., "Add feature" not "Added feature")
        3. Start with a type prefix (e.g., "feat:", "fix:", "docs:", "style:", "refactor:", "test:", "chore:")
        4. Limit the first line to 50 characters
        5. Add a blank line after the first line
        6. Provide more details in the body if needed
        """

    let generateCommitMessage (apiKey: string) (diff: string) =
        let client = new OpenAIAPI(apiKey)
        let messages = ResizeArray<ChatMessage>()
        messages.Add(ChatMessage(ChatMessageRole.System, systemPrompt))
        messages.Add(ChatMessage(ChatMessageRole.User, $"Generate a commit message for these changes:\n{diff}"))

        let request = ChatRequest(Model = "gpt-3.5-turbo", Messages = messages)
        let response = client.Chat.CreateChatCompletionAsync(request).Result
        response.Choices[0].Message.Content
