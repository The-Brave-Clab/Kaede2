using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.Assets.ScriptableObjects
{
    public abstract class BaseMasterData : ScriptableObject
    {
    }

    public enum CharacterId
    {
        Unknown = 0,
        [InspectorName("結城 友奈")]
        YuukiYuuna = 1,
        [InspectorName("東郷 美森")]
        TougouMimori = 2,
        [InspectorName("犬吠埼 風")]
        InubouzakiFuu = 3,
        [InspectorName("犬吠埼 樹")]
        InubouzakiItsuki = 4,
        [InspectorName("三好 夏凜")]
        MiyoshiKarin = 5,
        [InspectorName("鷲尾 須美")]
        WashioSumi = 6,
        [InspectorName("三ノ輪 銀")]
        MinowaGin = 7,
        [InspectorName("乃木 園子（小）")]
        NogiSonoko_S = 8,
        [InspectorName("乃木 若葉")]
        NogiWakaba = 9,
        [InspectorName("土居 球子")]
        DoiTamako = 10,
        [InspectorName("伊予島 杏")]
        IyojimaAnzu = 11,
        [InspectorName("郡 千景")]
        KooriChikage = 12,
        [InspectorName("高嶋 友奈")]
        TakashimaYuuna = 13,
        [InspectorName("白鳥 歌野")]
        ShiratoriUtano = 14,
        [InspectorName("乃木 園子（中）")]
        NogiSonoko_M = 15,
        [InspectorName("秋原 雪花")]
        AkiharaSekka = 16,
        [InspectorName("古波蔵 棗")]
        KohaguraNatsume = 17,
        [InspectorName("上里 ひなた")]
        UesatoHinata = 18,
        [InspectorName("藤森 美都")]
        FujimoriMito = 19,
        [InspectorName("赤嶺 友奈")]
        AkamineYuuna = 20,
        [InspectorName("国土 亜耶")]
        KokudoAya = 21,
        [InspectorName("楠 芽吹")]
        KusunokiMebuki = 22,
        [InspectorName("加賀城 雀")]
        KagajouSuzume = 23,
        [InspectorName("弥勒 夕海子")]
        MirokuYumiko = 24,
        [InspectorName("山伏 しずく")]
        YamabushiShizuku = 25,
        [InspectorName("山伏 シズク")]
        YamabushiShizuku_I = 26,
        [InspectorName("弥勒 蓮華")]
        MirokuRenge = 27,
        [InspectorName("桐生 静")]
        KiryuShizuka = 28,
        // 29?
        [InspectorName("安芸 真鈴")]
        AkiMasuzu = 30,
        [InspectorName("花本 美佳")]
        HanamotoYoshika = 31,
        [InspectorName("天馬 美咲")]
        TenmaMisaki = 32,
        [InspectorName("法花堂 姫")]
        HokkedouHime = 33,
        [InspectorName("芙蓉 友奈")]
        FuyouYuuna = 34,
        [InspectorName("柚木 友奈")]
        YuzukiYuuna = 35,
    }
}