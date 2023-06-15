using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerkPilot.Constants
{
    public static class AppConstants
    {
        public static readonly string ThoughtProcessPrompt = """
        Describe the thought process of answering the question using given question, explanation, keywords, information, and answer.

        ### EXAMPLE:
        Question: 'how many employees does Microsoft has now'

        Information: [google.pdf]: Microsoft has over 144,000 employees worldwide as of 2019.

        Answer: I don't know how many employees does Microsoft has now, but in 2019, Microsoft has over 144,000 employees worldwide.

        Summary:
        The question is about how many employees does Microsoft has now.
        To answer the question, I need to know the information of Microsoft and its employee number.
        I use keywords Microsoft, employee number to search information, and I find the following information:
         - [google.pdf] Microsoft has over 144,000 employees worldwide as of 2019.
        Using that information, I formalize the answer as
         - I don't know how many employees does Microsoft has now, but in 2019, Microsoft has over 144,000 employees worldwide.
        ###
        
        answer:
        {{$answer}}

        summary:
        """;

        public static readonly string ThoughtProcessPromptBackup = """
        Describe the thought process of answering the question using given question, explanation, keywords, information, and answer.

        ### EXAMPLE:
        Question: 'how many employees does Microsoft has now'

        Explanation: I need to know the information of Microsoft and its employee number.

        Keywords: Microsoft, employee number

        Information: [google.pdf]: Microsoft has over 144,000 employees worldwide as of 2019.

        Answer: I don't know how many employees does Microsoft has now, but in 2019, Microsoft has over 144,000 employees worldwide.

        Summary:
        The question is about how many employees does Microsoft has now.
        To answer the question, I need to know the information of Microsoft and its employee number.
        I use keywords Microsoft, employee number to search information, and I find the following information:
         - [google.pdf] Microsoft has over 144,000 employees worldwide as of 2019.
        Using that information, I formalize the answer as
         - I don't know how many employees does Microsoft has now, but in 2019, Microsoft has over 144,000 employees worldwide.
        ###

        question:
        {{$question}}

        explanation:
        {{$explanation}}

        keywords:
        {{$keywords}}

        information:
        {{$knowledge}}

        answer:
        {{$answer}}

        summary:
        """;


    }
}
