using System;
using System.Collections.Generic;

namespace ChatBot.Database.Models
{
    public class MessagePrompts
    {
        private readonly Random _random;

        public MessagePrompts()
        {
            _random = new Random();
        }

        public string GetRandomMessage(List<string> messages)
        {
            if (messages == null || messages.Count == 0)
            {
                throw new ArgumentException("Message list is empty or null");
            }

            int index = _random.Next(messages.Count);
            return messages[index];
        }

        public List<string> Greetings = new List<string>
        {
            "👋 Hello! Welcome to FirstBank. I’m Crowl, your virtual assistant. How can I help you?",
            "🌟 Welcome to FirstBank! I'm Crowl, here to assist you with any banking queries or tasks.",
            "🤖 Greetings! I'm Crowl, your friendly virtual assistant at FirstBank. How may I assist you today?",
            "👋 Hi there! It's a pleasure to welcome you to FirstBank. I'm Crowl, ready to help with any banking needs.",
            "🌟 Welcome aboard! I'm Crowl, your virtual assistant from FirstBank. Feel free to ask me anything!",
            "🤖 Hello and welcome! I'm Crowl, here to provide you with top-notch assistance at FirstBank.",
            "👋 Good day! I'm Crowl, your trusty virtual assistant at FirstBank. How can I make your day easier?",
            "🌟 Greetings and welcome! I'm Crowl, your helpful virtual assistant from FirstBank. How can I assist you today?",
            "🤖 Hi! It's great to have you here at FirstBank. I'm Crowl, ready to assist you with any banking tasks.",
            "👋 Welcome to FirstBank! I'm Crowl, your friendly AI assistant. How can I be of service today?",
            "🌟 Hello! I'm Crowl, your personal virtual banker at FirstBank. How may I assist you?",
            "🤖 Greetings! I'm Crowl, here to make your banking experience smoother. How can I help you today?",
            "👋 Hi! I'm Crowl, your reliable virtual assistant from FirstBank. What can I do for you?",
            "🌟 Welcome to FirstBank! I'm Crowl, here to provide you with personalized assistance. What do you need help with?",
            "🤖 Hello there! I'm Crowl, your digital banking companion. How can I assist you today?",
            "👋 Greetings! I'm Crowl, your virtual banking assistant. Feel free to ask me anything!",
            "🌟 Hi! I'm Crowl, your friendly neighborhood virtual banker. How can I assist you today?",
            "🤖 Welcome to FirstBank! I'm Crowl, your virtual assistant extraordinaire. How can I make your day better?",
            "👋 Hello and welcome! I'm Crowl, here to simplify your banking experience. What can I do for you?",
            "🌟 Hi there! I'm Crowl, your digital concierge at FirstBank. How may I assist you today?"
        };

        public List<string> GoodbyeMessages = new List<string>
        {
            "👋 Thank you for banking with us! Bye for now! To start another conversation, just say hi!",
            "🌟 Bye! Thank you for choosing FirstBank. If you need assistance later, simply say hi!",
            "🤖 Goodbye! It was a pleasure assisting you. Feel free to return anytime by saying hi!",
            "👋 Until next time! Thank you for banking with FirstBank. Say hi whenever you're ready to chat again!",
            "🌟 So long! We appreciate your business at FirstBank. If you have more questions, just say hi!",
            "🤖 Bye for now! Your satisfaction is important to us. Remember, saying hi starts a new conversation!",
            "👋 Bye-bye! We're glad to have been of service. If you ever need help, don't hesitate to say hi!",
            "🌟 Take care! Thank you for choosing FirstBank. Say hi whenever you'd like to chat again!",
            "🤖 Goodbye for now! We're here 24/7 at FirstBank. Don't forget, saying hi brings us back!",
            "👋 Bye and thank you! Your loyalty to FirstBank is appreciated. Need assistance? Just say hi!",
            "🌟 See you later! Thank you for banking with us. Anytime you're ready to chat, just say hi!",
            "🤖 Bye for now! It's been a pleasure serving you. Remember, saying hi keeps the conversation going!"
        };
        public List<string> ManageComplaintAuthSentences => new List<string>
        {
            "Let's take a moment to verify the account associated with your complaint.",
            "We just need a few seconds to confirm the account you'd like to address in your complaint.",
            "Let's quickly validate the account so we can proceed with managing your complaint.",
            "Taking a moment to verify the account will ensure we address your complaint accurately.",
            "We'll double-check the account details before proceeding with your complaint.",
            "Just a quick validation of your account before we move forward with managing your complaint.",
            "We'll make sure we have the right account details before addressing your complaint.",
            "Before we begin, let's confirm the account linked to your complaint.",
            "Verifying the account now to ensure we handle your complaint with care.",
            "Let's verify the account information to provide the best support for your complaint."
        };

        public List<string> ManageComplaintSelectOptionsSentences => new List<string>
        {
            "📝 So, would you like to make a complaint or track the status of one you've made before?",
            "🤔 Are you here to file a new complaint or check on an existing one?",
            "📋 Ready to file a complaint or just checking on a previous one? Let me know!",
            "🔍 Need to report a problem or follow up on an existing one? I'm here to assist!",
            "👋 Hi there! Are you looking to lodge a new complaint or follow up on an existing one?",
            "💬 Welcome! Are you seeking to submit a new complaint or check the status of an old one?",
            "📢 Hey! Ready to raise a complaint or need an update on a previous one? Let's get started!",
            "🙋‍♂️ Need help with a new complaint or want to know how things are going with your old one?",
            "📣 Hello! Are you interested in making a new complaint or checking on an existing one?",
            "❓ Want to share a new complaint or inquire about the status of a past one? Let me know!"
        };

        public List<string> DidNotUnderstandSentences => new List<string>
        {
            "🤔 Oops! It seems I didn't catch that. Can you try rephrasing your question?",
            "🔄 Sorry, I didn't quite understand. Could you ask in a different way, please?",
            "🙈 Oops! My bad. Can you try wording that differently for me?",
            "🔄 It appears I'm a bit confused. Could you try asking your question another way?",
            "🤔 Hmm, didn't quite catch that. Could you rephrase your question for me?",
            "🔄 Looks like I missed something. Can you try asking in a different way?",
            "🙊 Oops! I'm a bit lost. Can you try asking your question again?",
            "🔄 My apologies, but I didn't quite catch that. Can you try asking in a different way?",
            "🤔 Hmm, I'm not quite sure what you mean. Can you try explaining it differently?",
            "🔄 Sorry, I didn't get that. Could you try asking your question in a different way?"
        };

        public List<string> ManageFurtherComplaintSentences => new List<string>
        {
            "📝 Would you like to manage or log another complaint?",
            "🛠️ Ready to manage or log another complaint?",
            "📋 Want to handle or log another complaint?",
            "🔧 Need to manage or log another complaint?",
            "📝 Looking to manage or log another complaint?",
            "🔧 Shall we tackle or log another complaint?",
            "📋 Interested in managing or logging another complaint?",
            "🛠️ Ready to take care of or log another complaint?",
            "📝 Up for managing or logging another complaint?",
            "🔧 Want to deal with or log another complaint?"
        };

        public List<string> ManageComplaintGratitudeMessages => new List<string>
        {
            "🌟 Thank you for your patience and understanding! 🌟 " +
            "We sincerely apologize for any inconvenience you may have experienced. " +
            "Your satisfaction is our top priority, and we're constantly working to enhance your experience. 🚀 " +
            "Please don't hesitate to contact us if you have any more questions or concerns. We're here to assist you! 🤝 " +
            "Once again, thank you for choosing our services. Your support means the world to us! 💙",

            "🌟 We appreciate your patience and understanding! 🌟 " +
            "We're sorry for any inconvenience you may have encountered. " +
            "We prioritize your satisfaction and strive to improve our services continuously. 🚀 " +
            "Feel free to reach out if you need further assistance. We're always here to help! 🤝 " +
            "Thank you for choosing us. Your support is invaluable! 💙",

            "🌟 Thank you for your patience and understanding! 🌟 " +
            "We apologize for any inconvenience caused. " +
            "Your satisfaction is our utmost priority, and we're committed to making your experience better. 🚀 " +
            "If you have any questions or concerns, please reach out to us. We're here to support you! 🤝 " +
            "We're grateful for your continued trust in our services. Thank you for being our valued customer! 💙"
        };

        public List<string> TransactionRefRequestsMessages => new List<string>
        {
            "🔍 Could you please provide the Transaction Reference number? It typically starts with 'TRX...' ",
            "🔍 May I have the Transaction Reference number? It usually begins with 'TRX...' ",
            "🔍 Could you kindly share the Transaction Reference number? It typically starts with 'TRX...' ",
            "🔍 Do you mind sharing the Transaction Reference number? It usually begins with 'TRX...' ",
            "🔍 Could you provide the Transaction Reference number? It typically starts with 'TRX...' ",
            "🔍 May I request the Transaction Reference number? It usually begins with 'TRX...' ",
            "🔍 Would you be able to provide the Transaction Reference number? It typically starts with 'TRX...' ",
            "🔍 I'll need the Transaction Reference number, please. It typically starts with 'TRX...' "
        };

        public List<string> TransactionNotFoundMessages => new List<string>
        {
            "🤔 I'm having a hard time finding the transaction...\n\n" +
            "Would you like to provide a different Transaction Reference number?",
            "🔍 Hmm... I couldn't locate the transaction...\n\n" +
            "Would you mind giving me another Transaction Reference number?",
            "🔎 It seems I couldn't find the transaction...\n\n" +
            "Would you be able to provide a different Transaction Reference number?",
            "🚫 Sorry, I couldn't find any information for the transaction...\n\n" +
            "Do you have another Transaction Reference number you could share?",
            "🤷‍♂️ Oops! The transaction seems to be elusive...\n\n" +
            "Could you try giving me a different Transaction Reference number?"
        };

        public List<string> LogComplaintChallengesMessages => new List<string>
        {
            "📝 Before I log your complaint, please tell me, what challenges did you face with this transaction?",
            "🤔 Before we proceed with your complaint, could you share the challenges you encountered with this transaction?",
            "🚫 Before I file your complaint, could you explain the difficulties you experienced with this transaction?",
            "📋 Before logging your complaint, it would be helpful to understand the issues you faced with this transaction.",
            "💬 Before I proceed to document your complaint, could you describe the issues you encountered with this transaction?",
            "🔄 Before we move forward with your complaint, could you elaborate on the problems you faced during this transaction?",
            "🔍 Before I document your complaint, could you provide more details about the issues you encountered with this transaction?",
            "🛑 Before proceeding with your complaint, I need to understand the challenges you faced with this transaction.",
            "❓ Before I proceed, could you clarify the issues you experienced with this transaction?",
            "🔒 Before logging your complaint, please share the challenges you encountered with this transaction.",
            "📝 Before I proceed with your complaint, could you outline the difficulties you faced with this transaction?",
            "🔄 Before continuing, could you provide more information about the challenges you faced with this transaction?",
            "🔍 Before documenting your complaint, please describe the issues you encountered with this transaction.",
            "🛑 Before proceeding with your complaint, I need more information about the challenges you faced.",
            "❓ Before I log your complaint, could you provide additional details about the issues you encountered?"
        };

        public List<string> RetryTransactionMessages => new List<string>
        {
            "🙈 Oops! Sorry about that. Let's try another transaction.",
            "🔄 Hmm... It seems there was an issue. Let's try another transaction.",
            "🛑 Oops! My mistake. Let's attempt another transaction.",
            "🔄 Sorry for the confusion. Let's give another transaction a shot.",
            "🔍 Oops! It looks like there was an error. Let's try another transaction.",
            "🙊 My apologies. Let's move on to another transaction.",
            "🔄 Whoops! That didn't work. Let's try a different transaction.",
            "🔍 Oh no! It seems there was an issue. Let's try another transaction.",
            "🔄 Sorry about that. Let's attempt a different transaction.",
            "🛑 Uh-oh! It seems we hit a snag. Let's try another transaction.",
            "🔄 Apologies for the inconvenience. Let's try another transaction.",
            "🔍 Oops! Something went wrong. Let's try a different transaction.",
            "🔄 My apologies. Let's give another transaction a try.",
            "🛑 Sorry about the confusion. Let's move on to another transaction.",
            "🔄 Oops! Let's try another transaction to resolve this."
        };

        public List<string> LogComplaintApologyMessages = new List<string>
        {
            "🥺 I'm really sorry you had to experience any difficulties at all.",
            "😔 I apologize for any inconvenience caused by the situation.",
            "😞 I'm truly sorry for the trouble you've encountered.",
            "😟 I'm sorry to hear about the challenges you faced.",
            "😢 I apologize for any frustration this may have caused.",
            "😣 Sorry for the inconvenience! Let's work together to resolve this.",
            "😥 I'm sorry you're having trouble. Let's see how we can help.",
            "😩 Apologies for any inconvenience. Let's find a solution.",
            "😓 I'm sorry things didn't go smoothly. Let's try to make it right.",
            "😞 I'm sorry you're having issues. Let's work on fixing them together."
        };

        public List<string> LogComplaintWaitMessages = new List<string>
        {
            "📝 Please hold on while I log your complaint...",
            "📝 Let me take a moment to log your complaint...",
            "📝 Just a moment while I record your complaint...",
            "📝 Please wait while I document your complaint...",
            "📝 Hold tight! I'm about to log your complaint...",
            "📝 Hang on! Logging your complaint now...",
            "📝 Your complaint is being logged as we speak...",
            "📝 One moment please, I'm logging your complaint...",
            "📝 Sit tight! Logging your complaint...",
            "📝 Just a second while I put in your complaint...",
        };

        public List<string> AdditionalComplaintPromptMessages = new List<string>
        {
            "🤔 Would you like to log another complaint for this account?",
            $"📝 Do you have any other complaints to log for this account?",
            "📋 Are there any additional complaints you'd like to register for this account?",
            $"🔍 Is there another complaint you need to report for this account?",
            "🛠️ Do you have any other issues you'd like to bring to our attention for this account?",
            "📋 Need to register another complaint for this account?",
            "🔍 Are there any more complaints you'd like to log for this account?",
            "🤔 Interested in filing another complaint for this account?",
            $"📝 Do you want to log another complaint for this account?",
            "🔍 Need to add another complaint for this account?"
        };

        public List<string> ComplaintNumberRequestMessages = new List<string>
        {
            "What's the Complaint Number? The one starting with 'COMP...'",
            "Could you please provide the Complaint Number? It usually starts with 'COMP...'",
            "May I have the Complaint Number? It typically begins with 'COMP...'",
            "Could you kindly share the Complaint Number? It usually starts with 'COMP...'",
            "Do you mind sharing the Complaint Number? It typically begins with 'COMP...'",
            "Could you provide the Complaint Number? It typically starts with 'COMP...'",
            "May I request the Complaint Number? It usually begins with 'COMP...'",
            "Would you be able to provide the Complaint Number? It typically starts with 'COMP...'",
            "I'll need the Complaint Number, please. It typically starts with 'COMP...'"
        };

        public List<string> ComplaintNotResolvedMessages = new List<string>
        {
            "🥺 I'm so sorry your complaint is yet to be resolved. I promise we're working on it as much as we can.",
            "🥺 I apologize for the inconvenience of your complaint remaining unresolved. We're working hard to fix it.",
            "🥺 We understand your frustration that your complaint is still open. We're committed to resolving it as quickly as possible.",
            "🥺 I'm sorry your complaint hasn't been resolved yet. Please bear with us as we work to fix the issue.",
            "🥺 We apologize for the delay in resolving your complaint. We're prioritizing it and will update you as soon as possible.",
            "🥺 We're truly sorry your complaint is still pending. We assure you, it's receiving our full attention.",
            "🥺 I understand your concern that your complaint is still open. Please know that we're actively working to resolve it.",
            "🥺 I'm sorry to hear your complaint hasn't been resolved yet. We're working diligently to address it.",
            "🥺 We apologize for the inconvenience of your complaint remaining unresolved. We're working hard to fix it.",
            "🥺 We understand your frustration that your complaint is still open. We're committed to resolving it as quickly as possible."
        };

        public List<string> ComplaintRetrievalErrorMessages = new List<string>
            {
            "Ouch! An error occurred while I was trying to retrieve the complaint. I don't think any complaint exists with that number on this account.",
            "Oops! Something went wrong while retrieving the complaint. It seems there's no complaint associated with that number on this account.",
            "Uh-oh! There was an error retrieving the complaint. It appears there's no complaint with that number linked to this account.",
            "Oh no! It looks like I encountered an error while trying to find the complaint. It seems there's no matching complaint on this account.",
            "Yikes! There was a problem retrieving the complaint. It appears there's no complaint with that number registered under this account.",
            "Oh dear! An error occurred while retrieving the complaint. It seems there's no complaint linked to that number on this account."
            };

        public List<string> AdditionalComplaintRequestMessages = new List<string>
        {
            "Is there any other complaint you would like to track on this account?",
            "Would you like to track another complaint on this account?",
            "Do you have any other complaints you'd like to track on this account?",
            "Are there any additional complaints you want to track on this account?",
            "Do you need to monitor the status of another complaint on this account?",
            "Would you like to add another complaint for tracking on this account?",
            "Is there another complaint you wish to keep an eye on for this account?",
            "Do you have any other complaints you'd like to follow up on for this account?",
            "Are there additional complaints you'd like to track on this account?",
            "Would you like to track the status of another complaint linked to this account?"
        };

        public List<string> IssueResolvedMessages = new List<string>
        {
            "🎉 This issue has been resolved. Thanks for bearing with us. 😊",
            "🌟 Great news! The issue has been resolved. Thank you for your patience. 🙌",
            "🚀 We're happy to inform you that the issue has been resolved. Thank you for your understanding. 😊",
            "🎉 Good news! The issue you reported has been resolved. Thank you for your cooperation. 🌟",
            "🛠️ We've successfully resolved the issue. Thank you for your patience and understanding. 😊",
            "✨ The problem has been fixed. We appreciate your patience and cooperation. 🌟",
            "🌟 We're pleased to let you know that the issue has been resolved. Thank you for your patience. 😊",
            "🎊 The issue has been resolved. Thank you for bringing it to our attention. 🌟",
            "🎉 You'll be glad to know that the issue has been resolved. Thank you for your cooperation. 😊",
            "🚀 We're happy to report that the issue has been resolved. Thank you for your understanding. 😊"
        };






    }
}
