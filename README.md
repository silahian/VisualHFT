# VisualHFT

VisualHFT is a GUI for enterprise-level high-frequency trading systems. It focuses on visualizing market microstructure analytics, such as Limit Order Book dynamics, latencies, execution quality, and other analytics. This project is built with WPF and C# and is designed to support a high-frequency trading operation.


![Limit Order Book Visualization](https://github.com/silahian/VisualHFT/blob/master/docImages/LOB_imbalances_2.gif)
For this specific example we had the following:
- built with #wpf (desktop app)
- 5 levels of depth on each side
- market data is coming from Binance (btc/usd)
- red/green bubbles are selling/buying orders on the LOB
- light green/red bubbles are our orders (market making)
- bottom chart in blue, is the spread

## Getting started
To install and run the project, you need to:

1. Ensure the server app sends the required websocket messages.
2. Create a SQL Server database using the provided script.
3. Feed the system with the defined collection of JSON data.
4. Read positions and executions information from the database.
5. Run the core trading system located in the "demoTradingCore" folder.

Also, you can follow the detailed steps listed in [here](https://github.com/silahian/VisualHFT/issues/3)

## Long Description
VisualHFT is a comprehensive graphical user interface (GUI) designed to provide real-time insights into the operations of high-frequency trading systems. Built with WPF and C#, it serves as a powerful tool for visualizing market microstructure analytics, including Limit Order Book dynamics, latencies, execution quality, and other key metrics.

The primary function of VisualHFT is to offer a clear, real-time view of trading operations. It visualizes depth up to 5 levels on each side, displays real-time market data from many data sources, and provides a visualization of selling/buying orders on the Limit Order Book (LOB). It also shows the user's orders (market making) and provides a spread chart visualization.

VisualHFT operates by receiving a specific collection of JSON messages via WebSocket. These messages contain the real-time trading data that the GUI visualizes. The system requires a server application configured to send these messages, which include market data, order information, execution reports, position updates, and control messages.

The core trading system, located in the "demoTradingCore" folder, feeds data to VisualHFT. This system must have a REST and WebSocket server and be capable of persisting position data into the database. Please note that this console application is currently under development.

VisualHFT was open-sourced with the aim of contributing to the broader trading community and fostering innovation in the field of high-frequency trading. By providing a clear, real-time view of trading operations, VisualHFT enables users to make informed decisions and maintain control over their trading strategies.

## Features
- Market Depth visualization.
- Real-time market data from any source (see websocket incoming format).
- Visualization of your selling/buying orders on the LOB.
- Display your orders (market making).
- Spread chart visualization.
- more coming...

## About me
I’ve been building high-frequency trading software for the past 10 years. Primarily using C++, for the core system, which always runs in a collocated server next to the exchange.

I'm a passionate software engineer with a deep interest in the world of electronic trading. With extensive experience in the field, I have developed a keen understanding of the complexities and challenges that traders face in today's fast-paced, high-frequency trading environment.

My journey in electronic trading began with my work at a proprietary trading firm, where I was involved in developing and optimizing high-frequency trading systems. This experience gave me a firsthand look at the need for tools that provide real-time insights into trading operations, leading to the creation of VisualHFT.

In addition to my work in electronic trading, I have a broad background in software development, with skills in a range of programming languages and technologies. I am always eager to learn and explore new areas, and I believe in the power of open-source software to drive innovation and collaboration.

Through VisualHFT, I hope to contribute to the trading community and provide a valuable tool for traders and developers alike. I welcome feedback and contributions to the project and look forward to seeing how it evolves with the input of the community.


## History
The inception of VisualHFT was driven by a need for transparency and control in high-frequency trading operations. As the core high-frequency trading system operates in a collocated server with minimal human interaction, it was crucial to develop a mechanism that could provide real-time insights into the system's operations.

VisualHFT was designed as a visualization dashboard to fulfill this need. It provides a real-time view of the trading system's operations, including the volume and nature of orders being sent, the state of the market, and the ability to control some strategy parameters.

The goal was to create a tool that could offer a quick, comprehensive snapshot of what was happening in the trading system at any given moment. This allowed for more informed decision-making and improved operational control, even in a high-speed, automated trading environment.

Over time, VisualHFT has evolved to support a broader range of trading operations, not just high-frequency trading. However, its core purpose remains the same: to provide a clear, real-time view of trading operations, enabling users to make informed decisions and maintain control over their trading strategies.

## How to Install and Run the project

1. Prerequisites: Ensure you have the following software installed on your machine:
- .NET Framework 4.7.2 or later
- SQL Server 2019 or later
2. Clone the Repository: Clone the VisualHFT repository to your local machine using the following command in your terminal: git clone https://github.com/silahian/VisualHFT.git
3. Set Up the Database: Create a SQL Server database using the provided script located in the "database" folder of the project. Make sure to update the connection string in the project configuration to match your database settings. [SQL script](https://github.com/silahian/VisualHFT/blob/master/SQL%20scripts/table%20creation.sql)
4. Data Feeding: The system requires a specific collection of JSON data to operate. Ensure your server application is configured to send the required WebSocket messages.
5. Run the Core Trading System: Navigate to the "demoTradingCore" folder and run the core trading system. This system reads positions and executions information from the database and sends it to the GUI.
6. Start the GUI: Finally, navigate back to the root directory of the project and run the VisualHFT GUI. You should now be able to see real-time trading data in the GUI.

Please note that this project is designed to support a high-frequency trading operation. Make sure your trading system is compatible with VisualHFT before running the project.
### Data Feeding and WebSocket Messages
VisualHFT operates by receiving a specific collection of JSON messages via WebSocket. These messages contain the real-time trading data that the GUI visualizes.

To ensure the correct operation of VisualHFT, your server application must be configured to send the following types of WebSocket messages:
- Market Data Messages: These messages contain real-time market data from your trading system. They should include information such as the current bid and ask prices, the volume of orders at each price level, and any recent trades. [Market json](https://github.com/silahian/VisualHFT/blob/master/WS_input_json/Market.json)
- Order Messages: These messages provide information about the orders that your trading system is sending to the market. They should include details such as the order type (buy or sell), the order size, and the order price. [Active Orders json](https://github.com/silahian/VisualHFT/blob/master/WS_input_json/ActiveOrders.json)
- Exposure Messages: These messages provide updates on your trading system's current exposure. [Exposures json](https://github.com/silahian/VisualHFT/blob/master/WS_input_json/Exposures.json)
- Heartbeat Messages: These messages are sent at regular intervals to indicate that the trading system is running and connected. They typically don't contain any data but serve to confirm that the system is operational. [Heartbeat json](https://github.com/silahian/VisualHFT/blob/master/WS_input_json/HeartBeat.json)
- Strategy Messages: These messages provide information about the current state of your trading strategies. [Strategies json](https://github.com/silahian/VisualHFT/blob/master/WS_input_json/Strategies.json)

VisualHFT operates by receiving a specific collection of JSON messages via WebSocket for real-time data. This data includes market data messages, exposures, and active orders statuses. In addition to this real-time data, VisualHFT also requires access to positions and executions data, which are read from a Microsoft SQL Server database. 

Together, these data sources allow VisualHFT to function correctly and provide a comprehensive view of trading operations."


In addition to receiving data, VisualHFT also can communicate with the core trading system via REST. This allows VisualHFT to send specific commands back to the core system, enabling it to control various aspects of the trading operation. For instance, VisualHFT can instruct the core system to start or stop trading, or to modify parameters of active strategies. 

This two-way communication ensures that VisualHFT not only visualizes trading operations but also has the ability to influence them based on user inputs or predefined conditions.

The **demoTradingCore** is a crucial component of this project, serving as a demo trading engine. Its primary function is to feed data to the main system, **VisualHFT**. It's important to understand that **demoTradingCore** is not an actual trading system, but a tool designed to simulate and provide data for VisualHFT. This console application, which is currently under development, must be equipped with a REST and WebSocket server and should be capable of persisting position data into the database. 

To use it, navigate to the **demoTradingCore** folder and run the system. It will then start generating and sending data to VisualHFT, enabling you to visualize and analyze real-time trading operations in the main system. 

Remember, the spotlight of this project is on **VisualHFT** and its real-time analytics capabilities, with **demoTradingCore** serving as a supportive tool to feed it with necessary data."


![Architecture Diagram](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.002.png)


## Enterprise-Level Data Feed Integrations

VisualHFT is architecturally designed with a modular and extensible framework, making it an ideal solution for enterprise-level systems that rely on diverse data sources. Whether your infrastructure leverages sophisticated messaging systems like [Kafka](https://kafka.apache.org/), [RabbitMQ](https://www.rabbitmq.com/), the [FIX protocol via QuickFIX](http://www.quickfixengine.org/), or any other advanced data transmission method, VisualHFT stands ready to assimilate and visualize the data with precision. It's pertinent to note that, in this modular integration mode, the primary focus is on the visualization of market data.

### Key Features:

- **Modular Architecture**: VisualHFT's core is built around a plug-and-play model, allowing for seamless integration of new data feeds without disrupting existing functionalities.
  
- **Pre-built Data Retrievers**: We've incorporated robust data retrievers for industry-standard platforms such as [FIX](http://www.quickfixengine.org/) and [ZeroMQ](https://zeromq.org/), showcasing the platform's readiness for high-demand scenarios.

- **Order Retrieval & Trade History**: We've added a feature that allows the retrieval of past and current orders from the trading system, the history of trades executed. This could be in batch, or in real-time, depending on the implementation.

### Integration Steps for New Data Feeds (usually market data):

1. **Interface Implementation**: Begin by implementing the `IDataRetriever` interface. [See IDataRetriever.cs](https://github.com/silahian/VisualHFT/blob/master/DataRetriever/IDataSource.cs)
   
2. **Data Model Definition**: Craft a precise data model tailored to your specific feed, ensuring data integrity and compatibility.
   
3. **Data Processing & Event Handling**: Efficiently process the incoming data streams and subsequently invoke the `OnDataReceived` event to ensure real-time data visualization.

For enterprises looking to delve deeper and integrate custom solutions, our detailed implementations serve as comprehensive guides:
- [FIXDataRetriever](https://github.com/silahian/VisualHFT/blob/master/DataRetriever/FIXDataRetriever.cs)
- [ZeroMQDataRetriever](https://github.com/silahian/VisualHFT/blob/master/DataRetriever/ZeroMQDataRetriever.cs)
- [Apache Kafka](https://github.com/silahian/VisualHFT/blob/master/DataRetriever/KafkaDataRetriever.cs)

### Instantiating New Data Retrievers:

To integrate these data retrievers into your VisualHFT platform:

1. Navigate to the constructor of [Dashboard.xaml.cs](https://github.com/silahian/VisualHFT/blob/master/View/Dashboard.xaml.cs).
2. Instantiate your chosen data retriever, for example:
   ```csharp
   IDataRetriever fixDataRetriever = new FIXDataRetriever();
   ```
3. Ensure that the data retriever is started and stopped appropriately within the lifecycle of the `Dashboard` class.

### Integration Steps for New Order Retrievers (orders/trades):

To integrate a new source for order and trade retrieval, you'll need to follow these steps:

1. **Understand the Interface**: The `IDataTradeRetriever` interface is the cornerstone for integrating new order and trade sources. This interface provides two main events, `OnInitialLoad` and `OnDataReceived`, which are triggered when initial data is loaded and when new data is received, respectively.

2. **Choose an Existing Implementation**: We already provide three built-in implementations for this interface:
    - `EmptyTradesRetriever`: A no-op implementation that doesn't retrieve any data.
    - `MSSQLServerTradesRetriever`: Retrieves data from an MSSQL database.
    - `FIXTradesRetriever`: Reads and parses a FIX log continuously.

3. **Create Your Own Implementation**: If the existing implementations don't meet your needs, you can create your own by implementing the `IDataTradeRetriever` interface. Make sure to raise the `OnInitialLoad` and `OnDataReceived` events appropriately.

### Instantiating New Order Retrievers (orders/trades):

To use your new or existing order retriever, you'll need to instantiate it in the `HelperCommon.cs` file. Here's how:

1. **Open `HelperCommon.cs`**: Navigate to the `HelperCommon.cs` file where all settings are configured.

2. **Comment Out Existing Instantiation**: If there's an existing instantiation of `IDataTradeRetriever`, comment it out.
    ```csharp
    // public static IDataTradeRetriever EXECUTEDORDERS = new EmptyTradesRetriever();
    ```

3. **Add Your Implementation**: Instantiate your own implementation of `IDataTradeRetriever`.
    ```csharp
    public static IDataTradeRetriever EXECUTEDORDERS = new YourOwnTradeRetriever();
    ```

4. **Compile and Run**: After making these changes, compile and run the application to see your new data source in action.

   
**By leveraging VisualHFT's modular design, enterprises can ensure a streamlined integration process, making it a formidable tool in any high-frequency trading environment.**


## Screenshots

![Trading Statistics](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.003.png)
![Depth LOB](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.004.png)
![Analytics](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.005.png)
![Charts](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.006.png)
![Limit Order Book](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.007.png)
![Stats](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.008.png)

## Why I decided to open the project and the motivations
The decision to open-source VisualHFT was driven by a desire to contribute to the broader trading community and to foster innovation in the field of high-frequency trading.

Having worked extensively in the realm of electronic trading, I recognized the need for a tool that could provide real-time insights into the operations of a high-frequency trading system. While the core trading system operates with minimal human interaction, having a clear, real-time view of its operations is crucial for informed decision-making and effective control.

VisualHFT was developed to meet this need. However, I realized that its potential could be greatly amplified if it were open to the broader community. By open-sourcing VisualHFT, I aimed to provide a valuable resource for other traders and developers, enabling them to understand better and navigate the complex world of high-frequency trading.

Moreover, I believe that innovation thrives in a collaborative environment. By making VisualHFT open-source, I hope to encourage others to contribute their ideas and improvements, driving the project forward and ensuring it continues to evolve in line with the needs of the trading community.

## Things to improve
- documentation and wiki page.
- code architecture.
- be sure to maintain a MVVC pattern.
- be able to add more UI (ie: web-base UI)
- generalization of the strategy parameters and their UI elements.
- scalability (able to be used by multiple users at the same time)
- more real-time analytics and risk measurements
- add unit tests
- Ability to have a FIX network sniffer as input data (as an alternative to websockets)
- Security: even though these kinds of applications run inside a private network, there is no security at all involved.


## Contributing

If you are interested in reporting/fixing issues and contributing directly to the code base, please see [CONTRIBUTING.md](CONTRIBUTING.md) for more information on what we're looking for and how to get started.

> Important: We **will not accept** any changes to any of the existing input json message format. This is fixed and cannot be changed. The main reason for this is that we can break all existing installations of this system. Unless there is a “very strong” case that needs to be addressed, and all the community agrees upon that. However, we could accept having new json messages, to be parsed and processed accordingly, without breaking any of the existing ones.*


## How to contact me
For project questions use the repository’s forums or any of my social media profiles.
[Twitter](https://twitter.com/sisSoftware) | [LinkedIn](https://www.linkedin.com/in/silahian/) | Forums

