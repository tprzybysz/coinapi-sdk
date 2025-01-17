﻿using System;
using QuickFix;
using QuickFix.Fields;
using System.Collections.Generic;

namespace COINAPI.FIX.V2
{
    public class MarketDataApp : QuickFix.MessageCracker, QuickFix.IApplication
    {
        Session _session = null;

        private bool ExecuteSymbolListRequest = false;
        private string FilterBySymbolId = "DERIBIT_OPT_";
        private string FilterByExchangeName = "GEMINI";

        private string[] SubscribeBySymbolRegex = new string[] 
        {
            "GEMINI_SPOT_BTC_USD", // you can use the exact match of the symbol id
            "GEMINI_SPOT_(.*)_ETH", // or you can use the regular expression syntax to get eg. all spot symbols to ETH from GEMINI
            "GEMINI_SPOT_(BCH|LTC)_USD" // second example of the regex, the BCH/USD & LTC/USD
        };

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logon - " + sessionID.ToString());

            if (ExecuteSymbolListRequest)
            {
                // get all the symbols
                SecurityListRequestAll();

                // get symbols only by the symbol prefix
                if (!string.IsNullOrWhiteSpace(FilterBySymbolId))
                {
                    SecurityListRequestFilteredBySymbolIdPrefix(FilterBySymbolId);
                }

                // get symbols from specific exchange
                if (!string.IsNullOrWhiteSpace(FilterByExchangeName))
                {
                    SecurityListRequestFilteredByExchange(FilterByExchangeName);
                }
            }

            // subscribe by symbol ids
            //MarketDataRequestTrades(SubscribeBySymbolRegex);
            //MarketDataRequestQuotes(SubscribeBySymbolRegex);
            MarketDataRequestFullOrderbook(SubscribeBySymbolRegex);
        }

        private void MarketDataRequestTrades(string[] symbol_ids)
        {
            QuickFix.FIX44.MarketDataRequest mdr = new QuickFix.FIX44.MarketDataRequest();
            mdr.MDReqID = new MDReqID(Guid.NewGuid().ToString());
            mdr.SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES);
            mdr.MarketDepth = new MarketDepth(0);

            var type = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            type.MDEntryType = new MDEntryType(MDEntryType.TRADE);
            mdr.AddGroup(type);

            foreach (var symbol_id in symbol_ids)
            {
                var relatedsym = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
                relatedsym.Symbol = new Symbol(symbol_id);
                mdr.AddGroup(relatedsym);
            }

            SendMessage(mdr);
        }

        private void MarketDataRequestQuotes(string[] symbol_ids)
        {
            QuickFix.FIX44.MarketDataRequest mdr = new QuickFix.FIX44.MarketDataRequest();
            mdr.MDReqID = new MDReqID(Guid.NewGuid().ToString());
            mdr.SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES);
            mdr.MarketDepth = new MarketDepth(1);
            mdr.MDUpdateType = new MDUpdateType(MDUpdateType.FULL_REFRESH);

            {
                var type = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
                type.MDEntryType = new MDEntryType(MDEntryType.BID);
                mdr.AddGroup(type);
            }
            {
                var type = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
                type.MDEntryType = new MDEntryType(MDEntryType.OFFER);
                mdr.AddGroup(type);
            }

            foreach (var symbol_id in symbol_ids)
            {
                var relatedsym = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
                relatedsym.Symbol = new Symbol(symbol_id);
                mdr.AddGroup(relatedsym);
            }

            SendMessage(mdr);
        }

        private void MarketDataRequestFullOrderbook(string[] symbol_ids)
        {
            QuickFix.FIX44.MarketDataRequest mdr = new QuickFix.FIX44.MarketDataRequest();
            mdr.MDReqID = new MDReqID(Guid.NewGuid().ToString());
            mdr.SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES);
            mdr.MarketDepth = new MarketDepth(0);
            mdr.MDUpdateType = new MDUpdateType(MDUpdateType.INCREMENTAL_REFRESH);

            {
                var type = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
                type.MDEntryType = new MDEntryType(MDEntryType.BID);
                mdr.AddGroup(type);
            }
            {
                var type = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
                type.MDEntryType = new MDEntryType(MDEntryType.OFFER);
                mdr.AddGroup(type);
            }

            foreach (var symbol_id in symbol_ids)
            {
                var relatedsym = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
                relatedsym.Symbol = new Symbol(symbol_id);
                mdr.AddGroup(relatedsym);
            }

            SendMessage(mdr);
        }

        private void SecurityListRequestFilteredBySymbolIdPrefix(string symbol_id_prefix)
        {
            QuickFix.FIX44.SecurityListRequest slr =
                new QuickFix.FIX44.SecurityListRequest(
                    new SecurityReqID(Guid.NewGuid().ToString()),
                    new SecurityListRequestType(SecurityListRequestType.SYMBOL));
            slr.Symbol = new Symbol(symbol_id_prefix);
            SendMessage(slr);
        }

        private void SecurityListRequestFilteredByExchange(string exchange)
        {
            QuickFix.FIX44.SecurityListRequest slr =
                new QuickFix.FIX44.SecurityListRequest(
                    new SecurityReqID(Guid.NewGuid().ToString()),
                    new SecurityListRequestType(SecurityListRequestType.SYMBOL));
            slr.SecurityExchange = new SecurityExchange(exchange);
            SendMessage(slr);
        }

        private void SecurityListRequestAll()
        {
            QuickFix.FIX44.SecurityListRequest slr =
                new QuickFix.FIX44.SecurityListRequest(
                    new SecurityReqID(Guid.NewGuid().ToString()),
                    new SecurityListRequestType(SecurityListRequestType.ALL_SECURITIES));
            SendMessage(slr);
        }

        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine("Logout - " + sessionID.ToString());
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("FromAdmin - " + sessionID.ToString());
        }
        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("ToAdmin - " + sessionID.ToString());
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            //Console.WriteLine("IN:  " + message.ToString());
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            //Console.WriteLine("OUT: " + message.ToString());
        }
        #endregion

        public void OnMessage(QuickFix.FIX44.SecurityList msg,
            SessionID s)
        {
            for (int idx = 0; idx < msg.NoRelatedSym.getValue(); idx++)
            {
                var symbol = new QuickFix.FIX44.SecurityList.NoRelatedSymGroup();
                msg.GetGroup(idx + 1, symbol);

                Console.WriteLine($"Received symbol {symbol.Symbol} @ {symbol.SecurityExchange}");
            }
            Console.WriteLine($"--------------------------------------------------");
        }

        public void OnMessage(QuickFix.FIX44.MarketDataIncrementalRefresh msg, 
            SessionID s)
        {
            for (int idx = 0; idx < msg.NoMDEntries.getValue(); idx++)
            {
                var item = new QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup();
                msg.GetGroup(idx + 1, item);

                Console.WriteLine($"{item.MDEntryType} {item.MDUpdateAction} @ {item.Symbol}:");
                if (item.IsSetMDEntryID())
                {
                    Console.WriteLine($" ID: {item.MDEntryID}");
                }
                Console.WriteLine($" Date: {item.MDEntryDate}");
                Console.WriteLine($" Time: {item.MDEntryTime}");
                Console.WriteLine($" Px: {item.MDEntryPx}");
                Console.WriteLine($" Size: {item.MDEntrySize}");
                if (item.IsSetMDEntryOriginator())
                {
                    Console.WriteLine($" TakerSide: {item.MDEntryOriginator}");
                }
            }
            Console.WriteLine($"--------------------------------------------------");
        }

        public void OnMessage(QuickFix.FIX44.MarketDataSnapshotFullRefresh msg,
            SessionID s)
        {
            for (int idx = 0; idx < msg.NoMDEntries.getValue(); idx++)
            {
                var level = new QuickFix.FIX44.MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
                msg.GetGroup(idx + 1, level);
                Console.WriteLine($"{level.MDEntryType} @ {msg.Symbol}:");
                Console.WriteLine($" Date: {level.MDEntryDate}");
                Console.WriteLine($" Time: {level.MDEntryTime}");
                Console.WriteLine($" Px: {level.MDEntryPx}");
                Console.WriteLine($" Size: {level.MDEntrySize}");
            }
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
            {
                _session.Send(m);
            }
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }
    }
}