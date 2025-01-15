# Release Notes
See details [here](#release-notes-1)

# Coming Soon
- ability to connect to any market data source, including, equities, futures, forex, and even news feeds.
- open plug-in architecture, to allow 3rd party developers to create their analytics, data sources, and more
- new advanced studies around market microstructure.
- trading surveillance & infra monitoring
- and much more...

We are open to hearing from the community to add more features. Make sure you open new Issues with suggestions.


# VisualHFT

**VisualHFT** is a cutting-edge GUI platform for market analysis, focusing on real-time visualization of market microstructure. Built with WPF & C#, it displays key metrics like Limit Order Book dynamics and execution quality. Its modular design ensures adaptability for developers and traders, enabling tailored analytical solutions.

![Limit Order Book Visualization](https://github.com/silahian/VisualHFT/blob/master/docImages/LOB_imbalances_2.gif)

- built with #wpf (desktop app)
- 10 or more levels of depth on each side
- market data from any market data source
- ready to use from Binance, Okex (btc/usd) (soon plugins to add other sources)

## Getting started
To install and run the project, you need to:

1. Download the project
2. When compiling, make sure to reference the included plug-ins
3. Execute the solution.
4. You will need to choose the Provider (venue) and Symbol from the dropdowns.

## Long Description
**VisualHFT** is a state-of-the-art GUI platform engineered for sophisticated market analysis, emphasizing the real-time visualization of market microstructure. It is specifically designed to cater to the needs of both developers and traders seeking a deeper understanding of market dynamics.

Developed using WPF & C#, **VisualHFT** stands out for its ability to vividly display critical trading metrics such as Limit Order Book movements, transaction latencies, and execution quality. These features provide users with an unparalleled view into the intricacies of market behavior, aiding in strategic decision-making.

The platform's strength lies in its modular architecture, which allows for a high degree of customization through plugins. This flexibility makes **VisualHFT** an ideal solution for creating tailored market analysis tools that can adapt to various trading strategies and requirements.

**VisualHFT** not only delivers powerful analytics but also focuses on user accessibility. Its intuitive interface is designed to make complex data understandable, enabling users to quickly interpret and act on market insights.

Whether for monitoring trading performance, analyzing algorithmic strategies, or gaining comprehensive market insights, **VisualHFT** equips users with the tools necessary for a refined and informed trading experience.

## Features
- **Real-time market data from any source**: Add multiple market data using plugins.
- **Real-Time Market Microstructure Visualization**: Detailed view of market dynamics, including Limit Order Book movements.
- **Advanced Execution Quality Analysis**: Tools to assess and optimize trade execution and reduce slippage.
- **Interactive Charts and Graphs**: Dynamic and interactive visual representations of market data.
- **User-Centric Design**: An intuitive interface designed for ease of use, making complex data accessible.
- **Performance Metrics and Reporting**: Robust reporting tools to track and analyze trading performance metrics.
- more coming...

## About me
I’ve been building high-frequency trading software for the past 10 years. Primarily using C++, for the core system, which always runs in a collocated server next to the exchange.

I'm a passionate software engineer with a deep interest in the world of electronic trading. With extensive experience in the field, I have developed a keen understanding of the complexities and challenges that traders face in today's fast-paced, high-frequency trading environment.

My journey in electronic trading began with my work at a proprietary trading firm, where I was involved in developing and optimizing high-frequency trading systems. This experience gave me a firsthand look at the need for tools that provide real-time insights into trading operations, leading to the creation of **VisualHFT**.

In addition to my work in electronic trading, I have a broad background in software development, with skills in a range of programming languages and technologies. I am always eager to learn and explore new areas, and I believe in the power of open-source software to drive innovation and collaboration.

Through **VisualHFT**, I hope to contribute to the trading community and provide a valuable tool for traders and developers alike. I welcome feedback and contributions to the project and look forward to seeing how it evolves with the input of the community.


## History
The inception of **VisualHFT** was driven by a need for transparency and control in high-frequency trading operations. As the core high-frequency trading system operates in a collocated server with minimal human interaction, it was crucial to develop a mechanism that could provide real-time insights into the system's operations.

**VisualHFT** was designed as a visualization dashboard to fulfill this need. It provides a real-time view of the trading system's operations, including the volume and nature of orders being sent, the state of the market, and the ability to control some strategy parameters.

The goal was to create a tool that could offer a quick, comprehensive snapshot of what was happening in the trading system at any given moment. This allowed for more informed decision-making and improved operational control, even in a high-speed, automated trading environment.

Over time, **VisualHFT** has evolved to support a broader range of trading operations, not just high-frequency trading. However, its core purpose remains the same: to provide a clear, real-time view of trading operations, enabling users to make informed decisions and maintain control over their trading strategies.

## How to Install and Run the project

1. Prerequisites: Ensure you have the following software installed on your machine:
- .NET Framework 7.0
2. Clone the Repository: Clone the **VisualHFT** repository to your local machine using the following command in your terminal: git clone https://github.com/silahian/VisualHFT.git
3. Start the GUI: Finally, navigate to the root of the project and run the **VisualHFT** GUI. You should now be able to see real-time trading data in the GUI.


## Enterprise-Level Data Feed Integrations

VisualHFT is architecturally designed with a modular and extensible framework, making it an ideal solution for enterprise-level systems that rely on diverse data sources. Whether your infrastructure leverages sophisticated messaging systems like [Kafka](https://kafka.apache.org/), [RabbitMQ](https://www.rabbitmq.com/), the [FIX protocol via QuickFIX](http://www.quickfixengine.org/), or any other advanced data transmission method, VisualHFT stands ready to assimilate and visualize the data with precision. It's pertinent to note that, in this modular integration mode, the primary focus is on the visualization of market data.

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


# Release notes
### Jun 26 2024
**Enhancements**
- **Performance Improvements:**
  - Incorporated custom queues that improve performance and throughput by 40%.
  - Implemented custom object pools, enhancing memory allocation throughout the system.
- **Limit Order Book:**
  - Re-organized data structures and code for better usage, with significant improvements in performance and memory handling.
  - Optimized order book data structures for faster lookups and updates.
- **Plugins:**
  - Improved plugin lifecycle, allowing each plugin to have its own autonomy without affecting the core system (reconnection, auto stopping, etc.).
  - Enhanced error handling within plugins.
- **Notification Center:**
  - Introduced a new module to handle all exceptions and notifications from plugins and the core system without disrupting operations.
  - Improved UI experience for notifications.
- **Code Cleanup:**
  - Removed unused third-party packages and modules.
  - Refactored code to remove unnecessary database access from the core, now handled by plugins if needed.

These updates focus on enhancing system performance, reliability, and maintainability. For detailed changes, refer to [pull request #36](https://github.com/silahian/VisualHFT/pull/36).


### Oct 27 2023
**Enhancements**
- **Plugin Architecture**: Revamped the entire plug-in architecture. It is very easy to add new plugins to increase functionality.
- **Performance**: Improved performance by 200%. Refactored events and queues.


### Oct 19 2023

**Enhancements**
- **Memory Optimization with Object Pooling**: Introduced object pooling to reduce memory allocations in ProcessBufferedTrades method by reusing Trade and OrderBook objects.
- **Optimizing Real-Time Data Processing**: Replaced Task.Delay(0) with more efficient mechanisms like ManualResetEventSlim or BlockingCollection to handle high-frequency real-time data processing with lower latency and CPU usage.
- **Data Copy Optimization**: Implemented a CopyTo method to efficiently copy data between objects, facilitating object reuse and reducing memory allocations.
- **Converting Queue to BlockingCollection**: Transitioned from using Queue<IBinanceTrade> to BlockingCollection<IBinanceTrade> for thread-safe and efficient data processing in a multi-threaded environment.
- **Efficient Data Processing with BlockingCollection**: Utilized BlockingCollection<T> methods like Take and GetConsumingEnumerable to efficiently process data from different threads, ensuring thread-safety and reduced latency in high-frequency real-time analytic systems.


### Oct 02 2023

**New Features**
- **Plugin System Integration**: Incorporated the ability to use plugins within the application, allowing for modular expansion and customization. This will allow us to incorporate more functionalities in a modular fashion. Also, it will allow for 3rd party developers to expand VisualHFT even further.
- **Sample Plugins**: Added two sample plugins that serve as connectors to **Binance** and **Bitfinex** exchanges, demonstrating the capability and flexibility of the new plugin system. With this, VisualHFT won't need to run the **demoTradingCore** project anymore.
- **Plugin Manager UI**: Introduced a user interface for managing plugins. This allows users to load, unload, start, stop, and configure plugins.
- Plugin Normalization: Implemented a symbol normalization feature to allow users to analyze data from different exchanges in a standardized format. This ensures a consistent analysis across various exchanges.
- **Dynamic Plugin Settings UI**: Enhanced the plugin system to support dynamic user interface elements for plugin settings. This allows plugins to provide their own UI for configuration.
- **Performance Optimizations**: Introduced various performance improvements, including optimized data structures and multi-threading strategies.

**Enhancements**
- Improved Error Handling: Integrated more robust error handling mechanisms, especially for plugins. Plugins can now report errors which can either be logged or displayed to the user based on their severity.
- Base Class Refinements: Enhanced the base class for plugins to provide more features out-of-the-box, making it easier for third-party developers to create plugins.
- Tooltip for Symbol Normalization: Added detailed tooltips to guide users on how to use the symbol normalization feature.
- Code Refactoring: Refactored various parts of the code to improve maintainability, readability, and performance.


### Sep 22 2023
- Architectural improvement: Rearranged classes around to improve project structure.
- Improved performance overall:
    - Gradually separating pure “model” classes from “model view” classes. This will improve the MVVM architecture and it will give a performance boost, since model are light-weight.
    - Created custom collections and cache capability
    - UI updates improved for a flawless visualization
    - Improvement in memory usage
- Preparing the architecture, to introduce Plugins: these plugins will act as independent components, letting the community create new ones and have VisualHFT easily use them.
- Added Tiles into the dashboard, with different metrics. With the ability to launch realtime charts for each of them. The following list of metrics has been added:
    - VPIN
    - LOB Imbalance
    - TTO: trade to trade ratio
    - OTT: order to trade ratio
    - Market Resilience
    - Market Resilience Bias
- Multi Venue (providers) price chart
- Updated to latest .NET Framework .NET 7.0
