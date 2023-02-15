using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics; //for using assert
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace NFP
{
    public class MakeNFP // MakeNFP 클래스 : NFP 생성
    {
        #region .txt에서 읽어온 각 함재기들의 형상 Polygon 좌표(필요없음, 혹시 몰라서 저장)
        /*
        List<VERTEX> F35BPolygonCoor = new List<VERTEX>() // F35BPolygonCoor : F35BPolygon.txt에서 읽어온 STOVL 함재기 Polygon 좌표
        {   new VERTEX(-1.1999996900558472, -7.699999809265137),
            new VERTEX(-0.9999996423721314, -7.699999809265137),
            new VERTEX(-0.899999737739563, -7.399999618530273),
            new VERTEX(-0.899999737739563, -6.699999809265137),
            new VERTEX(-0.7999996542930603, -6.5),
            new VERTEX(-0.7999997138977051, -6.100000381469727),
            new VERTEX(0.5000002980232239, -6.100000381469727),
            new VERTEX(0.8000003695487976, -6.299999237060547),
            new VERTEX(-0.8000003099441528, -6.5),
            new VERTEX(0.9000003337860107, -6.799999237060547),
            new VERTEX(0.9000003337860107, -7.399999618530273),
            new VERTEX(1.0000004768371583, -7.699999809265137),
            new VERTEX(1.2000010013580323, -7.699999809265137),
            new VERTEX(1.5000004768371583, -7.600001335144043),
            new VERTEX(1.6000003814697266, -7.600000381469727),
            new VERTEX(1.9000004529953004, -7.5),
            new VERTEX(2.000000476837158, -7.5),
            new VERTEX(2.3000004291534426, -7.399999618530273),
            new VERTEX(2.500000238418579, -7.399999618530273),
            new VERTEX(2.700000286102295, -7.299999237060547),
            new VERTEX(2.6000003814697267, -7.299999237060547),
            new VERTEX(2.8000004291534426, -7.199999809265137),
            new VERTEX(3.3000009059906008, -7.199999809265137),
            new VERTEX(3.6000003814697267, -7.100000381469727),
            new VERTEX(3.6000003814697267, -6.100000381469727),
            new VERTEX(3.200002670288086, -5.90000057220459),
            new VERTEX(3.1000001430511476, -5.799999237060547),
            new VERTEX(2.700000286102295, -5.600000381469727),
            new VERTEX(2.5999879837036135, -5.499993324279785),
            new VERTEX(2.1999928951263429, -5.299983978271484),
            new VERTEX(2.000000238418579, -4.899999618530273),
            new VERTEX(1.9000009298324586, -4.600001335144043),
            new VERTEX(2.000000238418579, -4.5),
            new VERTEX(2.200000286102295, -4.400000095367432),
            new VERTEX(2.3000006675720217, -4.400000095367432),
            new VERTEX(2.500000476837158, -4.299999713897705),
            new VERTEX(2.700000286102295, -4.299999713897705),
            new VERTEX(3.0, -4.199999809265137),
            new VERTEX(3.099827766418457, -4.200044631958008),
            new VERTEX(3.3000001907348635, -4.100000381469727),
            new VERTEX(3.4000003337860109, -4.09999942779541),
            new VERTEX(3.6000003814697267, -4.0),
            new VERTEX(3.8000001907348635, -3.999999523162842),
            new VERTEX(4.000000953674316, -3.8999996185302736),
            new VERTEX(4.200000286102295, -3.9000000953674318),
            new VERTEX(4.500000476837158, -3.799999713897705),
            new VERTEX(4.599999904632568, -3.799999713897705),
            new VERTEX(4.800000190734863, -3.6999998092651369),
            new VERTEX(5.000000476837158, -3.700000286102295),
            new VERTEX(5.300000190734863, -3.599998950958252),
            new VERTEX(5.300000190734863, -3.0),
            new VERTEX(5.19999885559082, -2.6999993324279787),
            new VERTEX(5.299999713897705, -2.59999942779541),
            new VERTEX(5.300000190734863, -2.0),
            new VERTEX(5.100000381469727, -1.799999713897705),
            new VERTEX(5.100000381469727, -1.1999998092651368),
            new VERTEX(4.5, -1.5),
            new VERTEX(4.400001049041748, -1.3999996185302735),
            new VERTEX(4.200000762939453, -1.3000001907348633),
            new VERTEX(4.100000858306885, -1.1999998092651368),
            new VERTEX(3.90000057220459, -1.0999994277954102),
            new VERTEX(3.700000286102295, -0.8999996185302734),
            new VERTEX(3.500000476837158, -0.8000001907348633),
            new VERTEX(3.399998664855957, -0.6999988555908203),
            new VERTEX(3.200000286102295, -0.5999999046325684),
            new VERTEX(3.09999418258667, -0.49999570846557619),
            new VERTEX(2.9000027179718019, -0.40000104904174807),
            new VERTEX(2.8000006675720217, -0.2999999523162842),
            new VERTEX(2.6000051498413088, -0.20000314712524415),
            new VERTEX(2.4999983310699465, -0.09999847412109375),
            new VERTEX(2.3000004291534426, 2.384185791015625e-7),
            new VERTEX(2.1000003814697267, 0.20000028610229493),
            new VERTEX(1.8000004291534424, 0.8000004291534424),
            new VERTEX(1.7000004053115845, 1.1000003814697266),
            new VERTEX(1.700000286102295, 2.700000286102295),
            new VERTEX(1.6000003814697266, 3.000000238418579),
            new VERTEX(1.6000003814697266, 3.500000238418579),
            new VERTEX(1.4000003337860108, 3.700000286102295),
            new VERTEX(1.0000004768371583, 3.5),
            new VERTEX(0.8000003099441528, 3.9000003337860109),
            new VERTEX(0.8000003099441528, 4.000000476837158),
            new VERTEX(0.7000002861022949, 4.300000190734863),
            new VERTEX(0.699999988079071, 4.800002098083496),
            new VERTEX(0.6000002026557922, 5.100000858306885),
            new VERTEX(0.6000003218650818, 5.600000381469727),
            new VERTEX(0.5000003576278687, 5.90000057220459),
            new VERTEX(0.5000002980232239, 6.500000476837158),
            new VERTEX(0.40000030398368838, 6.800000190734863),
            new VERTEX(0.40000030398368838, 7.200000286102295),
            new VERTEX(0.10000029951334, 7.800000190734863),
            new VERTEX(2.980232238769531e-7, 7.90000057220459),
            new VERTEX(-0.09999970346689224, 7.800000190734863),
            new VERTEX(-0.3999997079372406, 7.200000286102295),
            new VERTEX(-0.3999996781349182, 6.700000286102295),
            new VERTEX(-0.4999997317790985, 6.500000476837158),
            new VERTEX(-0.4999997019767761, 5.800000190734863),
            new VERTEX(-0.599999725818634, 5.600000381469727),
            new VERTEX(-0.5999997854232788, 4.900001525878906),
            new VERTEX(-0.6999996900558472, 4.800000190734863),
            new VERTEX(-0.6999997496604919, 4.200000286102295),
            new VERTEX(-0.7999997735023499, 4.000000476837158),
            new VERTEX(-0.899999737739563, 3.700000286102295),
            new VERTEX(-0.9999997019767761, 3.500000238418579),
            new VERTEX(-1.1999996900558472, 3.4000003337860109),
            new VERTEX(-1.299999713897705, 3.500000238418579),
            new VERTEX(-1.399999737739563, 3.700000286102295),
            new VERTEX(-1.5999996662139893, 3.500000238418579),
            new VERTEX(-1.5999996662139893, 3.000000238418579),
            new VERTEX(-1.6999995708465577, 2.700000286102295),
            new VERTEX(-1.6999995708465577, 1.0000004768371583),
            new VERTEX(-2.0999996662139894, 0.20000028610229493),
            new VERTEX(-2.299999713897705, 0.0),
            new VERTEX(-2.499999761581421, -0.09999942779541016),
            new VERTEX(-2.5999996662139894, -0.19999980926513673),
            new VERTEX(-2.799999713897705, -0.299999475479126),
            new VERTEX(-2.8999998569488527, -0.39999961853027346),
            new VERTEX(-3.09999942779541, -0.4999997615814209),
            new VERTEX(-3.1999998092651369, -0.5999999046325684),
            new VERTEX(-3.3999998569488527, -0.6999998092651367),
            new VERTEX(-3.499999523162842, -0.7999992370605469),
            new VERTEX(-3.700042247772217, -0.9000287055969238),
            new VERTEX(-3.9000015258789064, -1.1000003814697266),
            new VERTEX(-4.099999904632568, -1.1999998092651368),
            new VERTEX(-4.200002193450928, -1.3000011444091797),
            new VERTEX(-4.399999618530273, -1.3999996185302735),
            new VERTEX(-4.500002861022949, -1.5000014305114747),
            new VERTEX(-4.699999809265137, -1.599998950958252),
            new VERTEX(-4.999999523162842, -1.3999996185302735),
            new VERTEX(-5.099999904632568, -1.1999998092651368),
            new VERTEX(-5.100000381469727, -1.5999994277954102),
            new VERTEX(-5.199997901916504, -1.9999985694885255),
            new VERTEX(-5.300000190734863, -2.0),
            new VERTEX(-5.299999713897705, -3.59999942779541),
            new VERTEX(-5.0, -3.6999998092651369),
            new VERTEX(-4.799999237060547, -3.6999998092651369),
            new VERTEX(-4.599999904632568, -3.8000001907348635),
            new VERTEX(-4.399999618530273, -3.799999713897705),
            new VERTEX(-4.199999809265137, -3.9000000953674318),
            new VERTEX(-3.999999761581421, -3.8999996185302736),
            new VERTEX(-3.799999475479126, -4.000000476837158),
            new VERTEX(-3.6999998092651369, -4.0),
            new VERTEX(-3.4000000953674318, -4.100000381469727),
            new VERTEX(-3.299999713897705, -4.099999904632568),
            new VERTEX(-3.0999810695648195, -4.200004577636719),
            new VERTEX(-2.8999998569488527, -4.199999809265137),
            new VERTEX(-2.6999998092651369, -4.300000190734863),
            new VERTEX(-2.499999523162842, -4.299999713897705),
            new VERTEX(-2.299999713897705, -4.399999618530273),
            new VERTEX(-2.1999998092651369, -4.399999618530273),
            new VERTEX(-1.999999761581421, -4.5),
            new VERTEX(-1.9000009298324586, -4.700002670288086),
            new VERTEX(-2.1999998092651369, -5.299999237060547),
            new VERTEX(-2.5999996662139894, -5.5),
            new VERTEX(-2.6999998092651369, -5.600000381469727),
            new VERTEX(-3.099989175796509, -5.799993515014648),
            new VERTEX(-3.199998378753662, -5.899999618530273),
            new VERTEX(-3.5999855995178224, -6.099992752075195),
            new VERTEX(-3.59999942779541, -7.100000381469727),
            new VERTEX(-3.299999713897705, -7.200000762939453),
            new VERTEX(-3.09999942779541, -7.199999809265137),
            new VERTEX(-2.9000000953674318, -7.299999237060547),
            new VERTEX(-2.799999475479126, -7.299999237060547),
            new VERTEX(-2.500007152557373, -7.399997711181641),
            new VERTEX(-2.1999998092651369, -7.399999618530273),
            new VERTEX(-1.999999761581421, -7.5),
            new VERTEX(-1.799999713897705, -7.5),
            new VERTEX(-1.5999996662139893, -7.600000381469727),
            new VERTEX(-1.499999761581421, -7.600000381469727) };

        List<VERTEX> HELOPolygonCoor = new List<VERTEX>() // HELOPolygonCoor : HELOPolygon.txt에서 읽어온 HELO 함재기 Polygon 좌표
        {   new VERTEX(6.9000396728515629, -1.699981689453125),
            new VERTEX(7.300025939941406, -1.6999778747558594),
            new VERTEX(7.600017547607422, -1.5999832153320313),
            new VERTEX(7.800025939941406, -1.5999832153320313),
            new VERTEX(7.800022125244141, -1.1999778747558594),
            new VERTEX(7.9000244140625, -1.0999832153320313),
            new VERTEX(7.800018310546875, -1.0999832153320313),
            new VERTEX(7.4000244140625, -1.199981689453125),
            new VERTEX(6.800025939941406, -1.199981689453125),
            new VERTEX(6.6000213623046879, -1.0999832153320313),
            new VERTEX(6.500026702880859, -1.0999794006347657),
            new VERTEX(6.300018310546875, -0.8999824523925781),
            new VERTEX(6.300025939941406, -0.6999778747558594),
            new VERTEX(6.500019073486328, -0.3999824523925781),
            new VERTEX(6.70001220703125, -0.4999809265136719),
            new VERTEX(6.9000244140625, -0.4999809265136719),
            new VERTEX(7.200019836425781, -0.5999794006347656),
            new VERTEX(7.4000244140625, -0.5999832153320313),
            new VERTEX(7.700023651123047, -0.4999809265136719),
            new VERTEX(7.9000244140625, -0.4999809265136719),
            new VERTEX(8.000022888183594, -0.199981689453125),
            new VERTEX(8.000022888183594, -0.09997940063476563),
            new VERTEX(7.700023651123047, 0.1000213623046875),
            new VERTEX(7.7000274658203129, 0.8000221252441406),
            new VERTEX(6.400028228759766, 0.8000221252441406),
            new VERTEX(6.200023651123047, 1.0000190734863282),
            new VERTEX(5.9000244140625, 1.0000190734863282),
            new VERTEX(5.600028991699219, 1.1000213623046876),
            new VERTEX(5.000026702880859, 1.1000213623046876),
            new VERTEX(4.600025177001953, 1.3000221252441407),
            new VERTEX(4.500022888183594, 1.4000205993652344),
            new VERTEX(4.2000274658203129, 1.6000213623046876),
            new VERTEX(4.100017547607422, 1.7000160217285157),
            new VERTEX(4.000026702880859, 1.7000198364257813),
            new VERTEX(3.7000274658203127, 1.1000213623046876),
            new VERTEX(3.3000221252441408, 0.8000221252441406),
            new VERTEX(3.100017547607422, 0.9000167846679688),
            new VERTEX(3.000019073486328, 0.9000205993652344),
            new VERTEX(2.8000221252441408, 1.0000228881835938),
            new VERTEX(2.600017547607422, 1.2000160217285157),
            new VERTEX(2.500019073486328, 1.2000160217285157),
            new VERTEX(2.200023651123047, 1.300018310546875),
            new VERTEX(2.2000274658203127, 1.4000205993652344),
            new VERTEX(2.1000213623046877, 1.4000205993652344),
            new VERTEX(1.8000221252441407, 1.5000190734863282),
            new VERTEX(-1.2999801635742188, 1.5000190734863282),
            new VERTEX(-1.4999809265136719, 1.2000160217285157),
            new VERTEX(-1.6999855041503907, 1.2000198364257813),
            new VERTEX(-1.8999824523925782, 1.3000221252441407),
            new VERTEX(-2.4999771118164064, 1.3000221252441407),
            new VERTEX(-2.699981689453125, 1.1000213623046876),
            new VERTEX(-2.999980926513672, 1.1000213623046876),
            new VERTEX(-3.2999801635742189, 1.2000198364257813),
            new VERTEX(-3.5999755859375, 1.0000228881835938),
            new VERTEX(-4.199985504150391, 0.7000198364257813),
            new VERTEX(-4.299983978271484, 0.6000175476074219),
            new VERTEX(-4.399982452392578, 0.40001678466796877),
            new VERTEX(-4.599971771240234, 0.3000221252441406),
            new VERTEX(-4.699974060058594, 0.3000221252441406),
            new VERTEX(-4.899974822998047, 0.20002365112304688),
            new VERTEX(-5.2999725341796879, 0.20002365112304688),
            new VERTEX(-5.299980163574219, -0.199981689453125),
            new VERTEX(-4.8999786376953129, -0.199981689453125),
            new VERTEX(-4.699985504150391, -0.29998016357421877),
            new VERTEX(-4.599979400634766, -0.29998016357421877),
            new VERTEX(-4.399982452392578, -0.3999824523925781),
            new VERTEX(-4.299980163574219, -0.5999794006347656),
            new VERTEX(-4.199977874755859, -0.699981689453125),
            new VERTEX(-3.5999794006347658, -0.9999847412109375),
            new VERTEX(-3.499980926513672, -0.9999809265136719),
            new VERTEX(-3.2999801635742189, -1.1999855041503907),
            new VERTEX(-2.899982452392578, -1.0999832153320313),
            new VERTEX(-2.5999832153320314, -1.0999832153320313),
            new VERTEX(-2.4999847412109377, -1.2999801635742188),
            new VERTEX(-1.8999786376953126, -1.2999763488769532),
            new VERTEX(-1.5999832153320313, -1.1999855041503907),
            new VERTEX(-1.3999786376953126, -1.199981689453125),
            new VERTEX(-1.2999839782714844, -1.3999824523925782),
            new VERTEX(-1.2999839782714844, -1.4999847412109376),
            new VERTEX(-0.6999855041503906, -1.4999847412109376),
            new VERTEX(-0.29998016357421877, -1.3999824523925782),
            new VERTEX(-0.09998703002929688, -1.3999862670898438),
            new VERTEX(0.300018310546875, -1.4999809265136719),
            new VERTEX(2.500019073486328, -1.4999847412109376),
            new VERTEX(2.800018310546875, -1.3999862670898438),
            new VERTEX(2.9000244140625, -1.3999824523925782),
            new VERTEX(3.200023651123047, -1.199981689453125),
            new VERTEX(3.4000244140625, -1.199981689453125),
            new VERTEX(3.600025177001953, -1.2999801635742188),
            new VERTEX(4.4000244140625, -1.2999801635742188),
            new VERTEX(4.500034332275391, -1.3999824523925782),
            new VERTEX(5.1000213623046879, -1.3999824523925782),
            new VERTEX(5.300037384033203, -1.4999885559082032),
            new VERTEX(5.900028228759766, -1.4999809265136719),
            new VERTEX(6.100059509277344, -1.5999832153320313),
            new VERTEX(6.7000274658203129, -1.5999832153320313)};
            */
    #endregion

        public static List<Tuple<VERTEX, VERTEX>> GetSTOVL(double s_Angle, double o_Angle, VERTEX s_location) // GetSTOVL() : 목표 함재기, 장애물 함재기 모두 STOVL 함재기
        {
            List<VERTEX> nfpPoly = new List<VERTEX>();
            VERTEX stationRefPt = s_location; // 장애물 함재기의 위치
            VERTEX orbitalRefPt = new VERTEX(8, 0, 0); //orbitalRefPt에 전투기 꼭지점 좌표기입
            //VERTEX orbitalRefPt = new VERTEX(8, 0, 0); //orbitalRefPt에 전투기 꼭지점 좌표기입
            //VERTEX orbitalRefPt = new VERTEX(0, 0, 0); //orbitalRefPt에 전투기 꼭지점 좌표기입

            double tmpX; double tmpY;
            tmpX = orbitalRefPt.X * Math.Cos(o_Angle * Math.PI / 180) - orbitalRefPt.Y * Math.Sin(o_Angle * Math.PI / 180);
            tmpY = orbitalRefPt.X * Math.Sin(o_Angle * Math.PI / 180) + orbitalRefPt.Y * Math.Cos(o_Angle * Math.PI / 180);

            orbitalRefPt.X = tmpX; orbitalRefPt.Y = tmpY;

            // S,M = 함재기 형상 넣기
            List<VERTEX> S = new List<VERTEX>(); // S = stationaryPolygon, 장애물 함재기 [STOVL 함재기]
            List<VERTEX> M = new List<VERTEX>(); // M = orbitalPolygon, 움직이려는 목표 함재기 [STOVL 함재기]

            #region @"C:\JsonData\Polygon_NPS\Result\" 경로에 있는 .json 파일 읽어오기
            string Sname = "F35BPolygon"; // 장애물(S) 함재기 = STOVL
            var Stmp = new List<VERTEX>();
            ConvertVector(Sname, ref Stmp);
            S = Stmp;

            string Mname = "F35BPolygon"; // 이동, 목표(M) 함재기 = STOVL
            var Mtmp = new List<VERTEX>();
            ConvertVector(Mname, ref Mtmp);
            M = Mtmp;
            #endregion

            S = Rotate(S, -90, new VERTEX(0, 0));
            M = Rotate(M, -90, new VERTEX(0, 0));
            S = Rotate(S, s_Angle, new VERTEX(0, 0));
            M = Rotate(M, o_Angle, new VERTEX(0, 0));

            for (int i = 0; i < S.Count; i++) S[i].Move(s_location.X, s_location.Y); // S[i].x + 장애물위치.x + S[i].y + 장애물위치.y

            //Marker.partialNFPList.Clear();
            List<Tuple<VERTEX, VERTEX>> RESULT = new List<Tuple<VERTEX, VERTEX>>();
            
            RESULT = Get(S, M, orbitalRefPt);

            return RESULT;
        }

        public static List<Tuple<VERTEX, VERTEX>> GetHELO(double s_Angle, double o_Angle, VERTEX s_location) // GetHELO() : 목표 함재기 STOVL 함재기, 장애물 함재기 HELO 함재기 
        {
            List<VERTEX> nfpPoly = new List<VERTEX>();
            VERTEX stationRefPt = s_location; // 장애물 함재기의 위치
            VERTEX orbitalRefPt = new VERTEX(8, 0, 0); //orbitalRefPt에 전투기 꼭지점 좌표기입
            double tmpX; double tmpY;
            tmpX = orbitalRefPt.X * Math.Cos(o_Angle * Math.PI / 180) - orbitalRefPt.Y * Math.Sin(o_Angle * Math.PI / 180);
            tmpY = orbitalRefPt.X * Math.Sin(o_Angle * Math.PI / 180) + orbitalRefPt.Y * Math.Cos(o_Angle * Math.PI / 180);

            orbitalRefPt.X = tmpX; orbitalRefPt.Y = tmpY;

            // S(장애물),M(이동할 목표) = 함재기 형상 넣기
            List<VERTEX> S = new List<VERTEX>(); // S = stationaryPolygon, 장애물 함재기 [HELO 함재기]
            List<VERTEX> M = new List<VERTEX>(); // M = orbitalPolygon, 움직이려는 목표 함재기 [STOVL 함재기]

            #region @"C:\JsonData\Polygon_NPS\Result\" 경로에 있는 .json 파일 읽어오기
            string Sname = "HELOPolygon"; // 장애물(S) 함재기 = HELO
            var Stmp = new List<VERTEX>();
            ConvertVector(Sname, ref Stmp);
            S = Stmp;

            string Mname = "F35BPolygon"; // 이동, 목표(M) 함재기 = STOVL
            var Mtmp = new List<VERTEX>();
            ConvertVector(Mname, ref Mtmp);
            M = Mtmp;
            #endregion

            //S = Rotate(S, -90, new VERTEX(0, 0)); // HELO 함재기는 안 돌려도 된다 <-> STOVL 함재기는 돌려야될껄..?
            M = Rotate(M, -90, new VERTEX(0, 0));
            S = Rotate(S, s_Angle, new VERTEX(0, 0));
            M = Rotate(M, o_Angle, new VERTEX(0, 0));

            for (int i = 0; i < S.Count; i++) S[i].Move(s_location.X, s_location.Y); // S[i].x + 장애물위치.x + S[i].y + 장애물위치.y

            return Get(S, M, orbitalRefPt);
        }

        public static List<VERTEX> Rotate(List<VERTEX> p, double theta, VERTEX RefPt)
        {
            var res = new List<VERTEX>();
            double X;
            double Y;
            for (int i = 0; i < p.Count; i++)
            {
                X = (p[i].X - RefPt.X) * Math.Cos(theta * Math.PI / 180) - (p[i].Y - RefPt.Y) * Math.Sin(theta * Math.PI / 180) + RefPt.X;
                Y = (p[i].X - RefPt.X) * Math.Sin(theta * Math.PI / 180) + (p[i].Y - RefPt.Y) * Math.Cos(theta * Math.PI / 180) + RefPt.Y;
                res.Add(new VERTEX(X, Y));
            }

            return res;
        }

        private static Root ReadJson(string fileName)
        {
            // .json 파일로 저장된 파일 읽어오기!
            StreamReader r = new StreamReader(@"C:\JsonData\Polygon_NPS\Result\" + fileName + ".json"); // 저장된 경로 + 파일 이름 + 확장자
            string jsonString = r.ReadToEnd();
            Root jsonFile = JsonConvert.DeserializeObject<Root>(jsonString);

            return jsonFile;
        }

        private static void ConvertVector(string name, ref List<VERTEX> list)
        {
            var j = ReadJson(name);

            for (int i = 0; i < j.vector.Count; i++)
            {
                list.Add(new VERTEX(j.vector[i].x, j.vector[i].y));
            }
        }

        public static List<Tuple<VERTEX, VERTEX>> Get(in List<VERTEX> s, in List<VERTEX> o, in VERTEX orbitalRefPt)
        {
            //Get NFP
            //bool IsRandRot = false;
            int Ns = s.Count;
            int No = o.Count;
            POLY_POINT[] tempS = new POLY_POINT[Ns];
            POLY_POINT[] tempO = new POLY_POINT[No];
            for (int i = 0; i < Ns; i++) tempS[i] = new POLY_POINT(s[i].X, s[i].Y, s[i].SweepAngle);
            for (int i = 0; i < No; i++) tempO[i] = new POLY_POINT(o[i].X, o[i].Y, o[i].SweepAngle);
                //Get Parts from NestingObjects
                    POLYGON polS = new POLYGON(tempS);
                    POLYGON polM = new POLYGON(tempO);
                    System.Diagnostics.Debug.Assert(Ns > 0);
                    System.Diagnostics.Debug.Assert(No > 0);
                    //Console.WriteLine($"NvS={polS.NPts}, NvM={polM.NPts}");
                //Random Rotate
                    //Random rand = new Random();
                    //double RandomRad = 0;
                    //if(IsRandRot) RandomRad = rand.Next(0,359)/180.0 * Math.PI;
                //Create NFP
                    int Qual=0;
                    int RefIdx = -1;
                    NFP P1P2 = new NFP(ref polS, ref polM);
                    POLYGON polNFP = P1P2.GetNFP_Orbital(true, ref Qual, ref RefIdx);
                //Result
                    List<Tuple<VERTEX, VERTEX>> result = new List<Tuple<VERTEX, VERTEX>>();
                    var tmpS = polNFP.Pts;
                    var tmpE = polNFP.PtX;
                    VERTEX ini = new VERTEX(0, 0);

                    var tmpRef = new VERTEX(tempO[RefIdx].x, tempO[RefIdx].y);
                    var refX = orbitalRefPt.X - tmpRef.X;   var refY = orbitalRefPt.Y - tmpRef.Y;
                    for (int i = 1; i < tmpS.Length; i++)
                    {
                        result.Add(new Tuple<VERTEX, VERTEX>(new VERTEX(tmpS[i].x + refX, tmpS[i].y + refY), new VERTEX(tmpE[i].x + refX, tmpE[i].y + refY)));
                        //X.DrawSegment(new VERTEX(tmpS[i].x, tmpS[i].y), new VERTEX(tmpE[i].x, tmpE[i].y),4);
                    }                    

                    return result;
                    //X.DrawPolygon(polS);
                    //X.DrawPolygon(polM);
                    //X.SendMsg("Draw NFP");
                    //X.DrawPolygon(polNFP);
                    //X.DrawPolygon(polNFP, 0, false);
                
            //End Action
            //X.SendMsg("-----------Code Terminated-------------",3);
            //    Console.WriteLine("---End Debug---");
            //    Console.ReadLine();//PAUSE
        }
    }
}