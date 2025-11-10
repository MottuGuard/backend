using backend.Models;

namespace backend.Tests.Helpers;

public class TestDataBuilder
{
    public static MotoBuilder Moto() => new MotoBuilder();
    public static UwbTagBuilder UwbTag() => new UwbTagBuilder();
    public static UwbAnchorBuilder UwbAnchor() => new UwbAnchorBuilder();
    public static PositionRecordBuilder PositionRecord() => new PositionRecordBuilder();
    public static ApplicationUserBuilder ApplicationUser() => new ApplicationUserBuilder();
}

public class MotoBuilder
{
    private readonly Moto _moto = new()
    {
        Chassi = "TEST-CHASSI-12345",
        Placa = "TST1234",
        Modelo = ModeloMoto.MottuSportESD,
        Status = MotoStatus.Disponivel,
        LastX = 5.0,
        LastY = 5.0,
        LastSeenAt = DateTime.UtcNow
    };

    public MotoBuilder WithId(int id)
    {
        _moto.Id = id;
        return this;
    }

    public MotoBuilder WithChassi(string chassi)
    {
        _moto.Chassi = chassi;
        return this;
    }

    public MotoBuilder WithPlaca(string placa)
    {
        _moto.Placa = placa;
        return this;
    }

    public MotoBuilder WithModelo(ModeloMoto modelo)
    {
        _moto.Modelo = modelo;
        return this;
    }

    public MotoBuilder WithStatus(MotoStatus status)
    {
        _moto.Status = status;
        return this;
    }

    public MotoBuilder WithPosition(double x, double y)
    {
        _moto.LastX = x;
        _moto.LastY = y;
        _moto.LastSeenAt = DateTime.UtcNow;
        return this;
    }

    public Moto Build() => _moto;
}

public class UwbTagBuilder
{
    private readonly UwbTag _tag = new()
    {
        Eui64 = "0000000000000001",
        Status = TagStatus.Ativa,
        MotoId = 1
    };

    public UwbTagBuilder WithId(int id)
    {
        _tag.Id = id;
        return this;
    }

    public UwbTagBuilder WithEui64(string eui64)
    {
        _tag.Eui64 = eui64;
        return this;
    }

    public UwbTagBuilder WithMotoId(int motoId)
    {
        _tag.MotoId = motoId;
        return this;
    }

    public UwbTagBuilder WithStatus(TagStatus status)
    {
        _tag.Status = status;
        return this;
    }

    public UwbTag Build() => _tag;
}

public class UwbAnchorBuilder
{
    private readonly UwbAnchor _anchor = new()
    {
        Name = "Test Anchor",
        X = 0.0,
        Y = 0.0,
        Z = 2.0
    };

    public UwbAnchorBuilder WithId(int id)
    {
        _anchor.Id = id;
        return this;
    }

    public UwbAnchorBuilder WithName(string name)
    {
        _anchor.Name = name;
        return this;
    }

    public UwbAnchorBuilder WithPosition(double x, double y, double z = 2.0)
    {
        _anchor.X = x;
        _anchor.Y = y;
        _anchor.Z = z;
        return this;
    }

    public UwbAnchor Build() => _anchor;
}

public class PositionRecordBuilder
{
    private readonly PositionRecord _record = new()
    {
        X = 5.0,
        Y = 5.0,
        Timestamp = DateTime.UtcNow
    };

    public PositionRecordBuilder WithMotoId(int motoId)
    {
        _record.MotoId = motoId;
        return this;
    }

    public PositionRecordBuilder WithPosition(double x, double y)
    {
        _record.X = x;
        _record.Y = y;
        return this;
    }

    public PositionRecordBuilder WithTimestamp(DateTime timestamp)
    {
        _record.Timestamp = timestamp;
        return this;
    }

    public PositionRecord Build() => _record;
}

public class ApplicationUserBuilder
{
    private readonly ApplicationUser _user = new()
    {
        UserName = "testuser@test.com",
        Email = "testuser@test.com",
        Name = "Test User",
        EmailConfirmed = true
    };

    public ApplicationUserBuilder WithId(int id)
    {
        _user.Id = id;
        return this;
    }

    public ApplicationUserBuilder WithEmail(string email)
    {
        _user.Email = email;
        _user.UserName = email;
        return this;
    }

    public ApplicationUserBuilder WithName(string name)
    {
        _user.Name = name;
        return this;
    }

    public ApplicationUser Build() => _user;
}
