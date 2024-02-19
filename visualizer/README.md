# Visualizer Scripts
This directory contains the C# Scripts used for Group 5's Visualizer component, for CG4002 CEG Capstone. It does NOT include the other assets used for the project.

The scripts and their purposes are:

## ARTrackingHandler.cs
Handles detection of enemy by using Vuforia to anchor onto image target

## ActionHandler.cs
Middle-man between GameplayUIManager and MQTTClient. Takes in "actions" from MQTTClient, executes them by passing them to GameplayUIManager.
Holds internal game state to display onto UI.

## GameStateManager.cs
Mini script to handle the transition between start screen and actual gameplay.

## GameplayUIManager.cs
Solely handles the display of all UI elements and AR effects on the visualizer. Takes in commands from ActionHandler to display things onto the UI.

## PreGameUIManager.cs
Handles the start screen UI and logic.

## MQTTClient.cs
Responsible for receiving and sending messages to the Game Engine, via MQTT. Specifically, sends visibility data and receives action and game state data.
