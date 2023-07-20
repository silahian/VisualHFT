# VisualHFT

VisualHFT is a GUI for enterprise-level high-frequency trading systems. It focuses on visualizing market microstructure analytics, such as Limit Order Book dynamics, latencies, execution quality, and other analytics. This project is built with WPF and C# and is designed to support a high-frequency trading operation.


![Limit Order Book Visualization](https://github.com/silahian/VisualHFT/blob/master/docImages/LOB_fulldepth.gif)
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


VisualHFT requires a specific collection of JSON data to function correctly. This data, which includes positions and executions, must be read from a Microsoft SQL Server database. Additionally, VisualHFT communicates with the core trading system via REST, sending commands such as start/stop trading and parameter changes.

The core trading system, which feeds data to VisualHFT, is located in the "demoTradingCore" folder. This system must have a REST and WebSocket server and be capable of persisting position data into the database. Please note that this console application is currently under development.

In the overall configuration, the core trading system, which is not included in this repository, functions as a WebSocket server. It sends data to VisualHFT and persists position data into the database, allowing VisualHFT to access and display this data.
![Architecture Diagram](/docImages/Aspose.Words.5b849bdf-d96d-4013-ad76-8c3daba3aead.002.png)

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

VisualHFT was developed to meet this need. However, I realized that its potential could be greatly amplified if it were open to the wider community. By open-sourcing VisualHFT, I aimed to provide a valuable resource for other traders and developers, enabling them to better understand and navigate the complex world of high-frequency trading.

Moreover, I believe that innovation thrives in a collaborative environment. By making VisualHFT open-source, I hope to encourage others to contribute their ideas and improvements, driving the project forward and ensuring it continues to evolve in line with the needs of the trading community.

## Things to improve
- currently using Telerik Charts. Replace them with some good opensource WPF charts.
- documentation and wiki page.
- code architecture.
- be sure to maintain a MVVC pattern.
- be able to add more UI (ie: web)
- throttling websocket input messages (when the server sends a lot of messages)
- performance (real-time charts are taking up too many resources).
- generalization of the strategy parameters and their UI elements.
- scalability (able to be used by multiple users at the same time)
- more real-time analytics and risk measurements
- server app for testing purposes (connect to binance, and simulate orders and executions)
- add unit tests
- Ability to have a FIX network sniffer as input data (as an alternative to websockets)
- Security: even though these kinds of applications run inside a private network, there is no security at all involved.

## How to contribute**
### follow the guidelines to contribute
[Setting guidelines for repository contributors - GitHub Docs](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors)

> Important: We **will not accept** any changes to any of the existing input json message format. This is fixed and cannot be changed. The main reason for this is that we can break all existing installations of this system. Unless there is a “very strong” case that needs to be addressed, and all the community agrees upon that. However, we could accept having new json messages, to be parsed and processed accordingly, without breaking any of the existing ones.*


## How to contact me
For project questions use the repository’s forums or any of my social media profiles.
[Twitter](https://twitter.com/sisSoftware) | [LinkedIn](https://www.linkedin.com/in/silahian/) | Forums

